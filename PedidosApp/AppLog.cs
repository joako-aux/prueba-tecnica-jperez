using System.Diagnostics;
using MySqlConnector;

namespace PedidosApp;

public static class AppLog
{
    public static void Write(string descripcion)
    {
        Debug.WriteLine("[FPIA AppLog.Write");
        try
        {
            if (descripcion.Length > 1000) descripcion = descripcion[..1000];
            using var cn = Db.Open();
            using var cmd = new MySqlCommand("INSERT INTO log (descripcion, fecha) VALUES (@d, NOW())", cn);
            cmd.Parameters.AddWithValue("@d", descripcion);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            
            Debug.WriteLine("No se pudo escribir en Log: " + ex.Message);
        }
    }

    public static void Error(string contexto, Exception ex)
    {
        Debug.WriteLine("[FPIA AppLog.Error");
        Write($"ERROR en {contexto}: {ex.Message}");
    }
}
