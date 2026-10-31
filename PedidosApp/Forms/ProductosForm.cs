using System.Diagnostics;

namespace PedidosApp.Forms;

public class ProductosForm : Form
{
    private readonly DataGridView dgv = Ui.Grid();
    private readonly TextBox txtDesc = new() { Width = 240 };
    private readonly NumericUpDown nudCant = new() { Width = 80, Maximum = 1_000_000 };
    private readonly NumericUpDown nudValor = new() { Width = 110, Maximum = 100_000_000, ThousandsSeparator = true };

    public ProductosForm()
    {
        Debug.WriteLine("[FPIA ProductosForm.ProductosForm");
        Text = "Productos";
        Size = new Size(900, 520);
        StartPosition = FormStartPosition.CenterParent;

        var btnCrear = new Button { Text = "Crear producto", AutoSize = true };
        var btnEstado = new Button { Text = "Activar / desactivar seleccionado", AutoSize = true };
        btnCrear.Click += (_, _) => Crear();
        btnEstado.Click += (_, _) => CambiarEstado();

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
        top.Controls.AddRange(new Control[]
        {
            Ui.Lbl("Descripción"), txtDesc, Ui.Lbl("Cantidad"), nudCant,
            Ui.Lbl("Valor venta"), nudValor, btnCrear, btnEstado
        });

        Controls.Add(dgv);
        Controls.Add(top);

        Load += (_, _) =>
        {
            SchemaChecker.Verify("Productos", "producto",
                new[] { "id", "descripcion", "cantidad", "valor_venta", "estado" });
            Recargar();
        };
    }

    private void Recargar()
    {
        Debug.WriteLine("[FPIA ProductosForm.Recargar");
        try { dgv.DataSource = ProductoRepo.List(); }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private void Crear()
    {
        Debug.WriteLine("[FPIA ProductosForm.Crear");
        if (string.IsNullOrWhiteSpace(txtDesc.Text)) { MessageBox.Show("Ingresa una descripción."); return; }
        if (nudValor.Value <= 0) { MessageBox.Show("El valor de venta debe ser mayor que 0."); return; }
        try
        {
            ProductoRepo.Create(txtDesc.Text.Trim(), (int)nudCant.Value, nudValor.Value);
            txtDesc.Clear(); nudCant.Value = 0; nudValor.Value = 0;
            Recargar();
        }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private void CambiarEstado()
    {
        Debug.WriteLine("[FPIA ProductosForm.CambiarEstado");
        if (dgv.CurrentRow == null) return;
        try
        {
            var id = Convert.ToInt32(dgv.CurrentRow.Cells["id"].Value);
            var activo = Convert.ToBoolean(dgv.CurrentRow.Cells["estado"].Value);
            ProductoRepo.SetEstado(id, !activo);
            Recargar();
        }
        catch (Exception ex) { Ui.Error(ex); }
    }
}
