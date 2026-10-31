using System.Diagnostics;
using System.Text;

namespace PedidosApp;

public class LineaPedido
{
    public int ProductoId { get; set; }
    public string Descripcion { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal => Cantidad * ValorUnitario;
}

public class PedidoResumen
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Observaciones { get; set; } = "";
    public string ClienteNombre { get; set; } = "";
    public string ClienteRut { get; set; } = "";
    public string ClienteDireccion { get; set; } = "";
    public string ClienteCorreo { get; set; } = "";
    public List<LineaPedido> Lineas { get; set; } = new();
    public decimal Total => Lineas.Sum(l => l.Subtotal);

    public string ToText()
    {
        Debug.WriteLine("[FPIA PedidoResumen.ToText");
        var sb = new StringBuilder();
        sb.AppendLine($"PEDIDO N° {Id}");
        sb.AppendLine($"Fecha: {Fecha:dd-MM-yyyy HH:mm}");
        sb.AppendLine($"Cliente: {ClienteNombre}");
        sb.AppendLine($"RUT: {ClienteRut}");
        sb.AppendLine($"Dirección: {ClienteDireccion}");
        sb.AppendLine($"Correo: {ClienteCorreo}");
        if (!string.IsNullOrWhiteSpace(Observaciones)) sb.AppendLine($"Obs.: {Observaciones}");
        sb.AppendLine(new string('-', 40));
        foreach (var l in Lineas)
            sb.AppendLine($"{l.Cantidad} x {l.Descripcion} @ {Formato.Money(l.ValorUnitario)} = {Formato.Money(l.Subtotal)}");
        sb.AppendLine(new string('-', 40));
        sb.AppendLine($"TOTAL: {Formato.Money(Total)}");
        return sb.ToString();
    }
}
