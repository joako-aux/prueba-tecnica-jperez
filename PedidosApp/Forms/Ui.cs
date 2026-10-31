using System.Diagnostics;

namespace PedidosApp.Forms;

public static class Ui
{
    public static DataGridView Grid()
    {
        Debug.WriteLine("[FPIA Ui.Grid");
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
    }

    public static Label Lbl(string texto)
    {
        Debug.WriteLine("[FPIA Ui.Lbl");
        return new Label { Text = texto, AutoSize = true, Margin = new Padding(8, 8, 3, 0) };
    }

    public static void Error(Exception ex)
    {
        Debug.WriteLine("[FPIA Ui.Error");
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
