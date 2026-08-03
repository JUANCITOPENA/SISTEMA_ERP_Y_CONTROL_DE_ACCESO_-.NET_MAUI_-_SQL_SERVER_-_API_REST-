using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CRUD_LOGIN_MAUI.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;

namespace CRUD_LOGIN_MAUI.Services
{
    public class TicketPdfService
    {
        public async Task<string> GenerarTicketPDFAsync(int numeroVenta, Cliente cliente, string vendedorNombre, List<DetalleVenta> carrito, decimal totalGeneral)
        {
            return await Task.Run(() =>
            {
                // Definir la ruta local en el dispositivo dependiendo del OS
                string fileName = $"Ticket_{numeroVenta}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                // FileSystem.CacheDirectory es ideal en MAUI para guardar archivos temporales como PDFs generados y abrirlos.
                string rutaPdf = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, fileName);

                using (var writer = new PdfWriter(rutaPdf))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        // Ancho de ticket térmico estándar de 80mm (~226 puntos de ancho)
                        PageSize rollSize = new PageSize(226, 800);
                        using (var document = new Document(pdf, rollSize))
                        {
                            // Márgenes reducidos para ticket térmico
                            document.SetMargins(10, 10, 10, 10);

                            // Encabezado
                            document.Add(new Paragraph("JPV PRO V2.0")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(14));

                            document.Add(new Paragraph("RNC: 101-12345-6")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(10));
                            
                            document.Add(new Paragraph("Av. Principal #123")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(10));
                                
                            document.Add(new Paragraph("Tel: 829-555-0000")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(10));

                            document.Add(new Paragraph("----------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            
                            // Información general
                            document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}").SetFontSize(10));
                            document.Add(new Paragraph($"Ticket: #{numeroVenta:D5}").SetFontSize(10));
                            document.Add(new Paragraph($"Cliente: {cliente.Nombre}").SetFontSize(10));
                            document.Add(new Paragraph($"Vendedor: {vendedorNombre}").SetFontSize(10));
                            
                            document.Add(new Paragraph("----------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            
                            // Tabla de Detalles
                            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 55, 15, 30 })).UseAllAvailableWidth();
                            
                            // Cabeceras de tabla
                            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("DESCRIPCIÓN").SetFontSize(10)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("CANT").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("TOT").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            
                            // Lista de productos iterados del carrito
                            foreach (var item in carrito)
                            {
                                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(item.ProductoNombre).SetFontSize(10)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph($"x{item.Cantidad}").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(item.Total.ToString("C")).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            }
                            
                            document.Add(table);
                            
                            document.Add(new Paragraph("----------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            
                            // Cálculos finales: SubTotal, ITBIS, TOTAL
                            // Dado que SQL calcula Itbis como Subtotal * 0.18 y Total como SubTotal * 1.18
                            decimal subTotal = totalGeneral / 1.18m;
                            decimal itbis = totalGeneral - subTotal;

                            // Total General
                            document.Add(new Paragraph($"SubTotal: {subTotal:C}")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetFontSize(10));
                                
                            document.Add(new Paragraph($"ITBIS 18%: {itbis:C}")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetFontSize(10));
                                
                            document.Add(new Paragraph($"TOTAL: {totalGeneral:C}")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetFontSize(12));
                            
                            document.Add(new Paragraph("----------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            
                            Paragraph footer = new Paragraph("¡GRACIAS POR SU\nCOMPRA!")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(10);
                            document.Add(footer);
                        }
                    }
                }
                
                return rutaPdf;
            });
        }
    }
}
