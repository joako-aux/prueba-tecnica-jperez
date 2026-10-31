using System.Diagnostics;

namespace PedidosApp.Forms;

public class ClientesForm : Form
{
    private readonly DataGridView dgv = Ui.Grid();
    private readonly TextBox txtNombre = new() { Width = 180 };
    private readonly TextBox txtRut = new() { Width = 100 };
    private readonly TextBox txtDireccion = new() { Width = 200 };
    private readonly TextBox txtCorreo = new() { Width = 180 };

    public ClientesForm()
    {
        Debug.WriteLine("[FPIA ClientesForm.ClientesForm");
        Text = "Clientes";
        Size = new Size(1000, 520);
        StartPosition = FormStartPosition.CenterParent;

        var btnCrear = new Button { Text = "Crear cliente", AutoSize = true };
        var btnEstado = new Button { Text = "Activar / desactivar seleccionado", AutoSize = true };
        btnCrear.Click += (_, _) => Crear();
        btnEstado.Click += (_, _) => CambiarEstado();

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
        top.Controls.AddRange(new Control[]
        {
            Ui.Lbl("Nombre"), txtNombre, Ui.Lbl("RUT"), txtRut, Ui.Lbl("Dirección"), txtDireccion,
            Ui.Lbl("Correo"), txtCorreo, btnCrear, btnEstado
        });

        Controls.Add(dgv);
        Controls.Add(top);

        Load += (_, _) =>
        {
            SchemaChecker.Verify("Clientes", "cliente",
                new[] { "id", "nombre", "rut", "direccion", "correo", "estado" });
            Recargar();
        };
    }

    private void Recargar()
    {
        Debug.WriteLine("[FPIA ClientesForm.Recargar");
        try { dgv.DataSource = ClienteRepo.List(); }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private void Crear()
    {
        Debug.WriteLine("[FPIA ClientesForm.Crear");
        if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtDireccion.Text))
        { MessageBox.Show("Nombre y dirección son obligatorios."); return; }
        if (!Formato.TryNormalizarRut(txtRut.Text, out var rut))
        { MessageBox.Show("RUT inválido."); return; }
        if (!Formato.CorreoValido(txtCorreo.Text.Trim()))
        { MessageBox.Show("Correo inválido."); return; }
        try
        {
            ClienteRepo.Create(txtNombre.Text.Trim(), rut, txtDireccion.Text.Trim(), txtCorreo.Text.Trim());
            txtNombre.Clear(); txtRut.Clear(); txtDireccion.Clear(); txtCorreo.Clear();
            Recargar();
        }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private void CambiarEstado()
    {
        Debug.WriteLine("[FPIA ClientesForm.CambiarEstado");
        if (dgv.CurrentRow == null) return;
        try
        {
            var id = Convert.ToInt32(dgv.CurrentRow.Cells["id"].Value);
            var activo = Convert.ToBoolean(dgv.CurrentRow.Cells["estado"].Value);
            ClienteRepo.SetEstado(id, !activo);
            Recargar();
        }
        catch (Exception ex) { Ui.Error(ex); }
    }
}
