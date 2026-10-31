using System.Diagnostics;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace PedidosApp;

public static class Formato
{
    private static readonly CultureInfo Cl = CultureInfo.GetCultureInfo("es-CL");

    public static string Money(decimal valor)
    {
        Debug.WriteLine("[FPIA Formato.Money");
        return valor.ToString("C0", Cl);
    }

    public static bool CorreoValido(string correo)
    {
        Debug.WriteLine("[FPIA Formato.CorreoValido");
        try { _ = new MailAddress(correo); return true; }
        catch { return false; }
    }

    
    public static bool TryNormalizarRut(string input, out string rut)
    {
        Debug.WriteLine("[FPIA Formato.TryNormalizarRut");
        rut = "";
        var s = input.Replace(".", "").Replace(" ", "").ToUpperInvariant();
        if (!s.Contains('-') && s.Length > 1) s = s[..^1] + "-" + s[^1];

        var m = Regex.Match(s, @"^(\d{1,8})-([\dK])$");
        if (!m.Success) return false;

        int suma = 0, mult = 2;
        foreach (var ch in m.Groups[1].Value.Reverse())
        {
            suma += (ch - '0') * mult;
            mult = mult == 7 ? 2 : mult + 1;
        }
        int resto = 11 - suma % 11;
        string dv = resto == 11 ? "0" : resto == 10 ? "K" : resto.ToString();
        if (dv != m.Groups[2].Value) return false;

        rut = s;
        return true;
    }
}
