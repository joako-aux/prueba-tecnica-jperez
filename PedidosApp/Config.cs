using System.Diagnostics;
using System.Text.Json;

namespace PedidosApp;

public static class Config
{
    public static string ConnectionString { get; } = Load();

    private static string Load()
    {
        Debug.WriteLine("[FPIA Config.Load");
        var env = Environment.GetEnvironmentVariable("FP_CONNECTION");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("Falta appsettings.json: copia appsettings.example.json y completa la contraseña.");

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return doc.RootElement.GetProperty("ConnectionString").GetString()!;
    }
}
