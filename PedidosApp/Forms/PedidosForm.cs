using System.ComponentModel;
using System.Data;
using System.Diagnostics;

namespace PedidosApp.Forms;

public class PedidosForm : Form
{
    private readonly ComboBox cmbCliente = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly TextBox txtObs = new() { Width = 400 };
    private readonly ComboBox cmbProducto = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly NumericUpDown nudCant = new() { Width = 70, Minimum = 1, Maximum = 1_000_000, Value = 1 };
    private readonly BindingList<LineaPedido> lineas = new();
    private readonly DataGridView dgv = Ui.Grid();
    private readonly Label lblTotal = new() { AutoSize = true, Text = "Total: $0", Margin = new Padding(8, 8, 20, 0) };
    private readonly TextBox txtResumen = new()
    {
        Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Right, Width = 380, Font = new Font("Consolas", 10)
    };
    private readonly Button btnPdf = new() { Text = "Generar PDF", AutoSize = true, Enabled = false };
    private PedidoResumen? ultimo;

    public PedidosForm()
    {
        Debug.WriteLine("[FPIA PedidosForm.PedidosForm");
        Text = "Pedidos";
        Size = new Size(1100, 600);
        StartPosition = FormStartPosition.CenterParent;
        dgv.DataSource = lineas;

        var btnAgregar = new Button { Text = "Agregar", AutoSize = true };
        var btnQuitar = new Button { Text = "Quitar seleccionado", AutoSize = true };
        var btnCrear = new Button { Text = "Crear pedido", AutoSize = true };
        btnAgregar.Click += (_, _) => Agregar();
        btnQuitar.Click += (_, _) => Quitar();
        btnCrear.Click += (_, _) => CrearPedido();
        btnPdf.Click += (_, _) => GenerarPdf();

        var fila1 = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
        fila1.Controls.AddRange(new Control[] { Ui.Lbl("Cliente"), cmbCliente, Ui.Lbl("Observaciones"), txtObs });

        var fila2 = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
        fila2.Controls.AddRange(new Control[] { Ui.Lbl("Producto"), cmbProducto, Ui.Lbl("Cantidad"), nudCant, btnAgregar, btnQuitar });

        var abajo = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
        abajo.Controls.AddRange(new Control[] { lblTotal, btnCrear, btnPdf });

        // Orden de adición: primero Fill, luego los bordes.
        Controls.Add(dgv);
        Controls.Add(txtResumen);
        Controls.Add(abajo);
        Controls.Add(fila2);
        Controls.Add(fila1);

        Load += (_, _) =>
        {
            SchemaChecker.Verify("Pedidos", "pedido", new[] { "id", "cliente_id", "fecha_pedido", "observaciones" });
            SchemaChecker.Verify("Pedidos", "pedido_detalle", new[] { "id", "pedido_id", "producto_id", "cantidad", "valor_unitario" });
            CargarCombos();
        };
    }

    private void CargarCombos()
    {
        Debug.WriteLine("[FPIA PedidosForm.CargarCombos");
        try
        {
            var dtC = ClienteRepo.List(true);
            dtC.Columns.Add("etiqueta", typeof(string), "nombre + ' (' + rut + ')'");
            cmbCliente.DisplayMember = "etiqueta";
            cmbCliente.ValueMember = "id";
            cmbCliente.DataSource = dtC;

            var dtP = ProductoRepo.List(true);
            dtP.Columns.Add("etiqueta", typeof(string), "descripcion + ' (stock ' + Convert(cantidad, 'System.String') + ')'");
            cmbProducto.DisplayMember = "etiqueta";
            cmbProducto.ValueMember = "id";
            cmbProducto.DataSource = dtP;
        }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private void ActualizarTotal()
    {
        Debug.WriteLine("[FPIA PedidosForm.ActualizarTotal");
        lblTotal.Text = "Total: " + Formato.Money(lineas.Sum(l => l.Subtotal));
    }

    private void Agregar()
    {
        Debug.WriteLine("[FPIA PedidosForm.Agregar");
        if (cmbProducto.SelectedItem is not DataRowView row) { MessageBox.Show("No hay productos activos."); return; }

        var id = Convert.ToInt32(row["id"]);
        var desc = row["descripcion"].ToString() ?? "";
        var valor = Convert.ToDecimal(row["valor_venta"]);
        var stock = Convert.ToInt32(row["cantidad"]);
        var cant = (int)nudCant.Value;

        var existente = lineas.FirstOrDefault(l => l.ProductoId == id);
        var total = cant + (existente?.Cantidad ?? 0);
        if (total > stock) { MessageBox.Show($"Stock insuficiente (disponible: {stock})."); return; }

        if (existente != null) lineas.Remove(existente);
        lineas.Add(new LineaPedido { ProductoId = id, Descripcion = desc, Cantidad = total, ValorUnitario = valor });
        ActualizarTotal();
    }

    private void Quitar()
    {
        Debug.WriteLine("[FPIA PedidosForm.Quitar");
        if (dgv.CurrentRow?.DataBoundItem is LineaPedido l) { lineas.Remove(l); ActualizarTotal(); }
    }

    private void CrearPedido()
    {
        Debug.WriteLine("[FPIA PedidosForm.CrearPedido");
        if (cmbCliente.SelectedValue == null) { MessageBox.Show("Selecciona un cliente activo."); return; }
        if (lineas.Count == 0) { MessageBox.Show("Agrega al menos un producto."); return; }
        try
        {
            var id = PedidoRepo.Create(Convert.ToInt32(cmbCliente.SelectedValue), txtObs.Text.Trim(), lineas);
            ultimo = PedidoRepo.GetResumen(id);
            txtResumen.Text = ultimo.ToText();
            btnPdf.Enabled = true;
            lineas.Clear();
            txtObs.Clear();
            ActualizarTotal();
            CargarCombos(); // refresca el stock mostrado
        }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private void GenerarPdf()
    {
        Debug.WriteLine("[FPIA PedidosForm.GenerarPdf");
        if (ultimo == null) return;
        using var dlg = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = $"Pedido_{ultimo.Id}.pdf" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            PdfService.Generar(ultimo, dlg.FileName);
            Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
        }
        catch (Exception ex) { Ui.Error(ex); }
    }
}
