using System.Diagnostics;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PedidosApp;

public static class PdfService
{
    public static void Generar(PedidoResumen r, string ruta)
    {
        Debug.WriteLine("[FPIA PdfService.Generar");
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(doc => doc.Page(page =>
            {
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));
                page.Header().Text($"Resumen de pedido N° {r.Id}").FontSize(20).Bold();

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text($"Fecha: {r.Fecha:dd-MM-yyyy HH:mm}");
                    col.Item().Text($"Cliente: {r.ClienteNombre}   |   RUT: {r.ClienteRut}");
                    col.Item().Text($"Dirección: {r.ClienteDireccion}");
                    col.Item().Text($"Correo: {r.ClienteCorreo}");
                    if (!string.IsNullOrWhiteSpace(r.Observaciones))
                        col.Item().Text($"Observaciones: {r.Observaciones}");

                    col.Item().PaddingTop(12).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(5);
                            c.ConstantColumn(60);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        t.Header(h =>
                        {
                            h.Cell().Element(Head).Text("Producto");
                            h.Cell().Element(Head).AlignRight().Text("Cant.");
                            h.Cell().Element(Head).AlignRight().Text("Valor unit.");
                            h.Cell().Element(Head).AlignRight().Text("Subtotal");
                        });

                        foreach (var l in r.Lineas)
                        {
                            t.Cell().Element(Body).Text(l.Descripcion);
                            t.Cell().Element(Body).AlignRight().Text(l.Cantidad.ToString());
                            t.Cell().Element(Body).AlignRight().Text(Formato.Money(l.ValorUnitario));
                            t.Cell().Element(Body).AlignRight().Text(Formato.Money(l.Subtotal));
                        }
                    });

                    col.Item().PaddingTop(10).AlignRight().Text($"Total: {Formato.Money(r.Total)}").FontSize(14).Bold();
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                });
            })).GeneratePdf(ruta);

            AppLog.Write($"PDF generado del pedido id={r.Id} en '{ruta}'");
        }
        catch (Exception ex) { AppLog.Error("PdfService.Generar", ex); throw; }
    }

    private static IContainer Head(IContainer c)
    {
        Debug.WriteLine("[FPIA PdfService.Head");
        return c.Background(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.Bold());
    }

    private static IContainer Body(IContainer c)
    {
        Debug.WriteLine("[FPIA PdfService.Body");
        return c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
    }
}
