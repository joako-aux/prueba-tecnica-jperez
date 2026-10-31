using System.Diagnostics;

namespace PedidosApp.Forms;

public class MainForm : Form
{
    public MainForm()
    {
        Debug.WriteLine("[FPIA MainForm.MainForm");
        Text = "Mini sistema de pedidos";
        Size = new Size(360, 300);
        StartPosition = FormStartPosition.CenterScreen;

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(30)
        };
        panel.Controls.Add(Boton("Productos", () => new ProductosForm().ShowDialog(this)));
        panel.Controls.Add(Boton("Clientes", () => new ClientesForm().ShowDialog(this)));
        panel.Controls.Add(Boton("Pedidos", () => new PedidosForm().ShowDialog(this)));
        Controls.Add(panel);
    }

    private static Button Boton(string texto, Action accion)
    {
        Debug.WriteLine("[FPIA MainForm.Boton");
        var b = new Button { Text = texto, Width = 260, Height = 45, Margin = new Padding(0, 0, 0, 12) };
        b.Click += (_, _) => accion();
        return b;
    }
}
