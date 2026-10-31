# Prueba técnica FullPyme — Mini sistema de pedidos (WinForms .NET 8)

Administración de productos y clientes, creación de pedidos y generación de PDF con el resumen.

## Requisitos
- Windows 10/11 (WinForms solo corre en Windows)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio Code con las extensiones **C# Dev Kit** (o **C#**) 
- Acceso a la base MySQL de Azure (SSL obligatorio)

## Puesta en marcha
1. Clonar el repositorio y abrir la carpeta en VS Code.
2. Crear las tablas ejecutando `sql/01_schema.sql` sobre el schema `prueba_jperez`
   (con el cliente `mysql`, MySQL Workbench o una extensión de MySQL en VS Code).
3. Copiar `PedidosApp/appsettings.example.json` a `PedidosApp/appsettings.json` y poner la contraseña.
   (`appsettings.json` está en `.gitignore`; alternativamente se puede usar la variable de entorno `FP_CONNECTION`).
4. Ejecutar:
   ```
   dotnet restore PedidosApp/PedidosApp.csproj
   dotnet run --project PedidosApp/PedidosApp.csproj
   ```
   O pulsar **F5** en VS Code (configuración "PedidosApp (WinForms)").

## Funcionamiento
- **Productos / Clientes:** crear y activar/desactivar. El RUT se valida (módulo 11) y se normaliza a mayúsculas, porque la collation `utf8_bin` distingue mayúsculas y minúsculas.
- **Pedidos:** elegir cliente activo y productos activos, validar stock, crear el pedido (transacción que descuenta stock), ver el resumen y generarlo en PDF (QuestPDF, licencia Community).
- **Log:** la tabla `log` registra creaciones, cambios de estado, errores, PDFs generados y el resultado de la verificación de esquema que cada módulo hace al abrirse (`INFORMATION_SCHEMA.COLUMNS`).
- **Opcional:** las grillas usan `SELECT *`, por lo que cualquier columna nueva agregada a una tabla aparece automáticamente.
- Cada clase y función incluye `Debug.WriteLine("[FPIA Clase.Metodo")` al inicio, según la regla de la prueba.

## Estructura
```
sql/01_schema.sql        Script de creación de tablas
PedidosApp/              Proyecto WinForms (.NET 8)
  Forms/                 Pantallas (interfaz creada por código, sin diseñador)
  Repositories.cs        Acceso a datos con try/catch y registro en Log
  PdfService.cs          Generación del PDF
```
