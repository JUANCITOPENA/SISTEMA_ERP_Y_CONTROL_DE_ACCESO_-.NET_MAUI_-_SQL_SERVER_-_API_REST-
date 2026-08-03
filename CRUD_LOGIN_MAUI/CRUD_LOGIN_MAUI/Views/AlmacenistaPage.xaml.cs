using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using CRUD_LOGIN_MAUI.Services;
using Microsoft.Maui.ApplicationModel;
using System.IO;

namespace CRUD_LOGIN_MAUI.Views
{
    public partial class AlmacenistaPage : ContentPage
    {
        private VentaService _service = new VentaService();
        private List<Dictionary<string, object>> _currentData = new List<Dictionary<string, object>>();

        public AlmacenistaPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                _currentData = await _service.GetReporteAlmacen();
                
                // Actualizar KPIs
                lblTotalProd.Text = _currentData.Count.ToString();
                
                decimal valorTotal = _currentData.Sum(d => Convert.ToDecimal(d["ValorInventario"]));
                lblValorInv.Text = valorTotal.ToString("C");
                
                int entradas = _currentData.Sum(d => Convert.ToInt32(d["StockInicialEntradas"]));
                lblTotalEntradas.Text = entradas.ToString("N0");
                
                int salidas = _currentData.Sum(d => Convert.ToInt32(d["TotalSalidas"]));
                lblTotalSalidas.Text = salidas.ToString("N0");

                // Asignar al CollectionView
                listaAlmacen.ItemsSource = _currentData;
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", "No se pudo cargar el reporte: " + ex.Message, "Entendido");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("🔒 Confirmación", "¿Estás seguro de que deseas cerrar tu sesión?", "Sí, salir", "No, quedarme");
            if (answer)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
        }

        private async void OnCrudClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("InventarioPage");
        }

        private async void OnPdfClicked(object sender, EventArgs e)
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                await DisplayAlert("⚠️ Vacío", "No hay datos para generar el reporte.", "OK");
                return;
            }

            try
            {
                string fileName = $"Reporte_Almacen_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                using (var writer = new iText.Kernel.Pdf.PdfWriter(filePath))
                {
                    using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                    {
                        var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);
                        document.SetMargins(30, 30, 30, 30);

                        // Cabecera
                        document.Add(new iText.Layout.Element.Paragraph("📦 SUPERMARKET JPV - ALMACÉN")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(16));
                        document.Add(new iText.Layout.Element.Paragraph("REPORTE DE ROTACIÓN DE INVENTARIO")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(14));
                        document.Add(new iText.Layout.Element.Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        document.Add(new iText.Layout.Element.Paragraph("-------------------------------------------------------------------------")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        // KPIs Globales
                        decimal totalValor = _currentData.Sum(d => Convert.ToDecimal(d["ValorInventario"]));
                        int totalEntradas = _currentData.Sum(d => Convert.ToInt32(d["StockInicialEntradas"]));
                        int totalSalidas = _currentData.Sum(d => Convert.ToInt32(d["TotalSalidas"]));

                        document.Add(new iText.Layout.Element.Paragraph($"TOTAL PRODUCTOS: {_currentData.Count} | VALOR INVENTARIO: {totalValor:C}").SetFontSize(12));
                        document.Add(new iText.Layout.Element.Paragraph($"TOTAL ENTRADAS: {totalEntradas} | TOTAL SALIDAS: {totalSalidas}").SetFontSize(12));
                        document.Add(new iText.Layout.Element.Paragraph("\n"));

                        // Tabla
                        iText.Layout.Element.Table table = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 30, 20, 12, 12, 12, 14 })).UseAllAvailableWidth();
                        
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRODUCTO").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("CATEGORÍA").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("ENTRADAS").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("SALIDAS").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("STOCK").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("VALOR").SetFontSize(10)));

                        foreach (var item in _currentData)
                        {
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["Producto"].ToString()).SetFontSize(9)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["Categoria"].ToString()).SetFontSize(9)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["StockInicialEntradas"].ToString()).SetFontSize(9).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["TotalSalidas"].ToString()).SetFontSize(9).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["StockActual"].ToString()).SetFontSize(9).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(Convert.ToDecimal(item["ValorInventario"]).ToString("C")).SetFontSize(9).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                        }
                        
                        document.Add(table);
                    }
                }

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath),
                    Title = "Reporte de Rotación Almacén"
                });

                await DisplayAlert("✅ Éxito", "Reporte de almacén generado y abierto.", "Genial");
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", $"No se pudo generar el PDF:\n{ex.Message}", "Entendido");
            }
        }
    }
}
