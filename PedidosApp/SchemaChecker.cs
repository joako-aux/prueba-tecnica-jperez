using System.Diagnostics;
using MySqlConnector;

namespace PedidosApp;

public static class SchemaChecker
{
    /// <summary>Verifica tabla y columnas en INFORMATION_SCHEMA.COLUMNS y registra el resultado en Log.</summary>
    public static bool Verify(string modulo, string tabla, string[] columnasEsperadas)
    {
        Debug.WriteLine("[FPIA SchemaChecker.Verify");
        try
        {
            using var cn = Db.Open();
            using var cmd = new MySqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t", cn);
            cmd.Parameters.AddWithValue("@t", tabla);

            var existentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var r = cmd.ExecuteReader())
                while (r.Read()) existentes.Add(r.GetString(0));

            if (existentes.Count == 0)
            {
                AppLog.Write($"[{modulo}] Verificación: la tabla '{tabla}' NO existe");
                return false;
            }

            var faltan = columnasEsperadas.Where(c => !existentes.Contains(c)).ToList();
            if (faltan.Count > 0)
            {
                AppLog.Write($"[{modulo}] Verificación: tabla '{tabla}' sin columnas: {string.Join(", ", faltan)}");
                return false;
            }

            AppLog.Write($"[{modulo}] Verificación OK: tabla '{tabla}' con {existentes.Count} columnas");
            return true;
        }
        catch (Exception ex)
        {
            AppLog.Error($"SchemaChecker.Verify({modulo})", ex);
            return false;
        }
    }
}
