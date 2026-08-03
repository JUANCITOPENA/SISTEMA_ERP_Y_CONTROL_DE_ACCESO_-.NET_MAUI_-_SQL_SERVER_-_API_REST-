using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CRUD_LOGIN_MAUI.Api.Models;

namespace CRUD_LOGIN_MAUI.Api.Services
{
    public class TicketPdfGenerator
    {
        public byte[] GenerarPdf(TicketRequest request)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(5, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, request));
                    page.Footer().AlignCenter().Text("¡Gracias por su compra!").SemiBold();
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("SUPERMERCADO JPV").FontSize(14).SemiBold();
                column.Item().AlignCenter().Text("RNC: 101-23456-7");
                column.Item().AlignCenter().Text("Av. Principal #123, SD");
                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
            });
        }

        private void ComposeContent(IContainer container, TicketRequest request)
        {
            container.Column(column =>
            {
                column.Item().Text($"Ticket #: {request.NumeroVenta}");
                column.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
                column.Item().Text($"Cliente: {request.ClienteNombre}");
                column.Item().Text($"Cajero: {request.VendedorNombre}");
                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);

                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Text("Desc").SemiBold();
                    row.RelativeItem(1).AlignRight().Text("Cant").SemiBold();
                    row.RelativeItem(2).AlignRight().Text("Prec").SemiBold();
                    row.RelativeItem(2).AlignRight().Text("Total").SemiBold();
                });

                foreach (var item in request.Detalles)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text(item.ProductoNombre);
                        row.RelativeItem(1).AlignRight().Text(item.Cantidad.ToString());
                        row.RelativeItem(2).AlignRight().Text(item.PrecioVentaAplicado.ToString("C"));
                        row.RelativeItem(2).AlignRight().Text(item.Total.ToString("C"));
                    });
                }

                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                column.Item().AlignRight().Text($"TOTAL: {request.TotalGeneral:C}").FontSize(12).SemiBold();
            });
        }
    }
}
