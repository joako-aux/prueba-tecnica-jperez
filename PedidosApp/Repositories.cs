using System.Data;
using System.Diagnostics;
using MySqlConnector;

namespace PedidosApp;

public static class ProductoRepo
{
    public static DataTable List(bool soloActivos = false)
    {
        Debug.WriteLine("[FPIA ProductoRepo.List");
        try
        {
            // SELECT * -> cualquier columna nueva de la tabla aparece sola en la grilla.
            return Db.Query("SELECT * FROM producto" + (soloActivos ? " WHERE estado = 1" : "") + " ORDER BY id");
        }
        catch (Exception ex) { AppLog.Error("ProductoRepo.List", ex); throw; }
    }

    public static long Create(string descripcion, int cantidad, decimal valorVenta)
    {
        Debug.WriteLine("[FPIA ProductoRepo.Create");
        try
        {
            using var cn = Db.Open();
            using var cmd = new MySqlCommand(
                "INSERT INTO producto (descripcion, cantidad, valor_venta, estado) VALUES (@d, @c, @v, 1)", cn);
            cmd.Parameters.AddWithValue("@d", descripcion);
            cmd.Parameters.AddWithValue("@c", cantidad);
            cmd.Parameters.AddWithValue("@v", valorVenta);
            cmd.ExecuteNonQuery();
            AppLog.Write($"Producto creado id={cmd.LastInsertedId} '{descripcion}'");
            return cmd.LastInsertedId;
        }
        catch (Exception ex) { AppLog.Error("ProductoRepo.Create", ex); throw; }
    }

    public static void SetEstado(int id, bool activo)
    {
        Debug.WriteLine("[FPIA ProductoRepo.SetEstado");
        try
        {
            using var cn = Db.Open();
            using var cmd = new MySqlCommand("UPDATE producto SET estado = @e WHERE id = @id", cn);
            cmd.Parameters.AddWithValue("@e", activo ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            AppLog.Write($"Producto id={id} cambió a {(activo ? "activo" : "inactivo")}");
        }
        catch (Exception ex) { AppLog.Error("ProductoRepo.SetEstado", ex); throw; }
    }
}

public static class ClienteRepo
{
    public static DataTable List(bool soloActivos = false)
    {
        Debug.WriteLine("[FPIA ClienteRepo.List");
        try
        {
            return Db.Query("SELECT * FROM cliente" + (soloActivos ? " WHERE estado = 1" : "") + " ORDER BY id");
        }
        catch (Exception ex) { AppLog.Error("ClienteRepo.List", ex); throw; }
    }

    public static long Create(string nombre, string rut, string direccion, string correo)
    {
        Debug.WriteLine("[FPIA ClienteRepo.Create");
        try
        {
            using var cn = Db.Open();
            using var cmd = new MySqlCommand(
                "INSERT INTO cliente (nombre, rut, direccion, correo, estado) VALUES (@n, @r, @d, @c, 1)", cn);
            cmd.Parameters.AddWithValue("@n", nombre);
            cmd.Parameters.AddWithValue("@r", rut);
            cmd.Parameters.AddWithValue("@d", direccion);
            cmd.Parameters.AddWithValue("@c", correo);
            cmd.ExecuteNonQuery();
            AppLog.Write($"Cliente creado id={cmd.LastInsertedId} rut={rut}");
            return cmd.LastInsertedId;
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            AppLog.Error("ClienteRepo.Create (RUT duplicado)", ex);
            throw new InvalidOperationException("Ya existe un cliente con ese RUT.");
        }
        catch (Exception ex) { AppLog.Error("ClienteRepo.Create", ex); throw; }
    }

    public static void SetEstado(int id, bool activo)
    {
        Debug.WriteLine("[FPIA ClienteRepo.SetEstado");
        try
        {
            using var cn = Db.Open();
            using var cmd = new MySqlCommand("UPDATE cliente SET estado = @e WHERE id = @id", cn);
            cmd.Parameters.AddWithValue("@e", activo ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            AppLog.Write($"Cliente id={id} cambió a {(activo ? "activo" : "inactivo")}");
        }
        catch (Exception ex) { AppLog.Error("ClienteRepo.SetEstado", ex); throw; }
    }
}

public static class PedidoRepo
{
    /// <summary>Crea el pedido y su detalle en una transacción, validando estado y stock, y descontando stock.</summary>
    public static int Create(int clienteId, string observaciones, IEnumerable<LineaPedido> lineas)
    {
        Debug.WriteLine("[FPIA PedidoRepo.Create");
        try
        {
            using var cn = Db.Open();
            using var tx = cn.BeginTransaction(); // si algo falla, al salir sin Commit se hace rollback

            using (var chk = new MySqlCommand("SELECT estado FROM cliente WHERE id = @id", cn, tx))
            {
                chk.Parameters.AddWithValue("@id", clienteId);
                var estado = chk.ExecuteScalar();
                if (estado == null || !Convert.ToBoolean(estado))
                    throw new InvalidOperationException("El cliente no existe o está inactivo.");
            }

            int pedidoId;
            using (var cmd = new MySqlCommand(
                "INSERT INTO pedido (cliente_id, fecha_pedido, observaciones) VALUES (@c, NOW(), @o)", cn, tx))
            {
                cmd.Parameters.AddWithValue("@c", clienteId);
                cmd.Parameters.AddWithValue("@o", string.IsNullOrWhiteSpace(observaciones) ? DBNull.Value : observaciones);
                cmd.ExecuteNonQuery();
                pedidoId = (int)cmd.LastInsertedId;
            }

            foreach (var l in lineas)
            {
                decimal valor; int stock; bool activo;
                using (var q = new MySqlCommand(
                    "SELECT valor_venta, cantidad, estado FROM producto WHERE id = @id FOR UPDATE", cn, tx))
                {
                    q.Parameters.AddWithValue("@id", l.ProductoId);
                    using var r = q.ExecuteReader();
                    if (!r.Read()) throw new InvalidOperationException($"El producto {l.ProductoId} no existe.");
                    valor = r.GetDecimal(0);
                    stock = r.GetInt32(1);
                    activo = r.GetBoolean(2);
                }

                if (!activo) throw new InvalidOperationException($"El producto '{l.Descripcion}' está inactivo.");
                if (l.Cantidad > stock) throw new InvalidOperationException($"Stock insuficiente para '{l.Descripcion}' (disponible: {stock}).");

                using (var ins = new MySqlCommand(
                    "INSERT INTO pedido_detalle (pedido_id, producto_id, cantidad, valor_unitario) VALUES (@p, @pr, @c, @v)", cn, tx))
                {
                    ins.Parameters.AddWithValue("@p", pedidoId);
                    ins.Parameters.AddWithValue("@pr", l.ProductoId);
                    ins.Parameters.AddWithValue("@c", l.Cantidad);
                    ins.Parameters.AddWithValue("@v", valor);
                    ins.ExecuteNonQuery();
                }

                using (var upd = new MySqlCommand("UPDATE producto SET cantidad = cantidad - @c WHERE id = @id", cn, tx))
                {
                    upd.Parameters.AddWithValue("@c", l.Cantidad);
                    upd.Parameters.AddWithValue("@id", l.ProductoId);
                    upd.ExecuteNonQuery();
                }
            }

            tx.Commit();
            AppLog.Write($"Pedido creado id={pedidoId} cliente_id={clienteId}");
            return pedidoId;
        }
        catch (Exception ex) { AppLog.Error("PedidoRepo.Create", ex); throw; }
    }

    public static PedidoResumen GetResumen(int pedidoId)
    {
        Debug.WriteLine("[FPIA PedidoRepo.GetResumen");
        try
        {
            using var cn = Db.Open();
            var res = new PedidoResumen { Id = pedidoId };

            using (var cmd = new MySqlCommand(
                @"SELECT p.fecha_pedido, p.observaciones, c.nombre, c.rut, c.direccion, c.correo
                  FROM pedido p JOIN cliente c ON c.id = p.cliente_id WHERE p.id = @id", cn))
            {
                cmd.Parameters.AddWithValue("@id", pedidoId);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) throw new InvalidOperationException($"El pedido {pedidoId} no existe.");
                string S(int i) => r.IsDBNull(i) ? "" : r.GetString(i);
                res.Fecha = r.GetDateTime(0);
                res.Observaciones = S(1);
                res.ClienteNombre = S(2);
                res.ClienteRut = S(3);
                res.ClienteDireccion = S(4);
                res.ClienteCorreo = S(5);
            }

            using (var cmd = new MySqlCommand(
                @"SELECT d.producto_id, pr.descripcion, d.cantidad, d.valor_unitario
                  FROM pedido_detalle d JOIN producto pr ON pr.id = d.producto_id
                  WHERE d.pedido_id = @id ORDER BY d.id", cn))
            {
                cmd.Parameters.AddWithValue("@id", pedidoId);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    res.Lineas.Add(new LineaPedido
                    {
                        ProductoId = r.GetInt32(0),
                        Descripcion = r.GetString(1),
                        Cantidad = r.GetInt32(2),
                        ValorUnitario = r.GetDecimal(3)
                    });
            }
            return res;
        }
        catch (Exception ex) { AppLog.Error("PedidoRepo.GetResumen", ex); throw; }
    }
}
