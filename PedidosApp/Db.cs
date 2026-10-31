using System.Data;
using System.Diagnostics;
using MySqlConnector;

namespace PedidosApp;

public static class Db
{
    public static MySqlConnection Open()
    {
        Debug.WriteLine("[FPIA Db.Open");
        var cn = new MySqlConnection(Config.ConnectionString);
        cn.Open();
        return cn;
    }

    public static DataTable Query(string sql)
    {
        Debug.WriteLine("[FPIA Db.Query");
        using var cn = Open();
        using var da = new MySqlDataAdapter(sql, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }
}
