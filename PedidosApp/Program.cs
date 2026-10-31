using System.Diagnostics;
using PedidosApp.Forms;

Debug.WriteLine("[FPIA Program.Main");
ApplicationConfiguration.Initialize();
AppLog.Write("Aplicación iniciada");
Application.Run(new MainForm());
