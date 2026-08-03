using CRUD_LOGIN_MAUI.Models;
using CRUD_LOGIN_MAUI.Services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace CRUD_LOGIN_MAUI.Views
{
    public partial class InventarioPage : ContentPage
    {
        VentaService service = new VentaService();
        Producto? productoSeleccionado;

        public InventarioPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarCategorias();
            CargarProductos();
        }

        private async void CargarCategorias()
        {
            pickerCategoria.ItemsSource = await service.GetCategorias();
        }

        private async void CargarProductos()
        {
            ListaProductos.ItemsSource = await service.GetProductosAsync();
        }

        private async void OnGuardar(object sender, EventArgs e)
        {
            try
            {
                var cat = pickerCategoria.SelectedItem as Categoria;
                if (cat == null || string.IsNullOrEmpty(txtNombre.Text)) return;
                var p = productoSeleccionado ?? new Producto();
                p.Nombre = txtNombre.Text;
                p.CategoriaId = cat.Id;
                p.PrecioCompra = decimal.Parse(txtPrecioCompra.Text);
                p.PrecioVenta = decimal.Parse(txtPrecioVenta.Text);
                p.Stock = int.Parse(txtStock.Text);
                await service.UpsertProducto(p);
                OnLimpiar(null, null);
                CargarProductos();
                await DisplayAlert("Éxito", "Producto guardado.", "OK");
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                productoSeleccionado = e.CurrentSelection.FirstOrDefault() as Producto;
                if (productoSeleccionado != null)
                {
                    txtNombre.Text = productoSeleccionado.Nombre;
                    txtPrecioCompra.Text = productoSeleccionado.PrecioCompra.ToString();
                    txtPrecioVenta.Text = productoSeleccionado.PrecioVenta.ToString();
                    txtStock.Text = productoSeleccionado.Stock.ToString();

                    var categorias = pickerCategoria.ItemsSource as List<Categoria>;
                    if (categorias != null)
                    {
                        pickerCategoria.SelectedIndex = categorias.FindIndex(x => x.Id == productoSeleccionado.CategoriaId);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error en Selección", ex.Message, "OK");
            }
        }

        private void OnLimpiar(object? sender, EventArgs? e)
        {
            txtNombre.Text = txtPrecioCompra.Text = txtPrecioVenta.Text = txtStock.Text = "";
            pickerCategoria.SelectedIndex = -1;
            productoSeleccionado = null;
            ListaProductos.SelectedItem = null;
        }

        private async void OnEliminar(object sender, EventArgs e)
        {
            if (productoSeleccionado == null || productoSeleccionado.Id == 0)
            {
                await DisplayAlert("Atención", "Seleccione un producto de la lista para eliminar.", "OK");
                return;
            }

            bool answer = await DisplayAlert("Confirmar", $"¿Está seguro de eliminar '{productoSeleccionado.Nombre}'?\n\n(No se borrará si tiene ventas asociadas)", "Sí, eliminar", "No");
            if (answer)
            {
                try
                {
                    var result = await service.DeleteProductoAsync(productoSeleccionado.Id);
                    if (result.Exito)
                    {
                        await DisplayAlert("Éxito", result.Mensaje, "OK");
                        OnLimpiar(null, null);
                        CargarProductos();
                    }
                    else
                    {
                        await DisplayAlert("Restricción", result.Mensaje, "Entendido");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = e.NewTextValue?.ToLowerInvariant();
            var allProducts = await service.GetProductosAsync();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ListaProductos.ItemsSource = allProducts;
            }
            else
            {
                ListaProductos.ItemsSource = allProducts.Where(p => 
                    (p.Nombre != null && p.Nombre.ToLowerInvariant().Contains(keyword)) ||
                    (p.CategoriaNombre != null && p.CategoriaNombre.ToLowerInvariant().Contains(keyword))
                ).ToList();
            }
        }

        // ====================== GENERACIÓN DE REPORTE PDF ======================
        private async void OnGenerarPDF(object sender, EventArgs e)
        {
            try
            {
                var productos = await service.GetProductosAsync();
                if (productos == null || productos.Count == 0)
                {
                    await DisplayAlert("Sin datos", "No hay productos en el inventario.", "OK");
                    return;
                }

                string fileName = $"Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, fileName);

                using (var writer = new iText.Kernel.Pdf.PdfWriter(filePath))
                {
                    using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                    {
                        var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);
                        document.SetMargins(30, 30, 30, 30);

                        document.Add(new iText.Layout.Element.Paragraph("SUPERMARKET JPV")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(16));
                        document.Add(new iText.Layout.Element.Paragraph("REPORTE DE INVENTARIO")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(14));
                        document.Add(new iText.Layout.Element.Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        document.Add(new iText.Layout.Element.Paragraph("--------------------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        iText.Layout.Element.Table table = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 35, 25, 10, 15, 15 })).UseAllAvailableWidth();
                        
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRODUCTO").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("CATEGORIA").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("STOCK").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("COSTO").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRECIO V.").SetFontSize(10)));

                        foreach (var prod in productos)
                        {
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.Nombre).SetFontSize(10)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.CategoriaNombre ?? "Sin categoría").SetFontSize(10)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.Stock.ToString()).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.PrecioCompra.ToString("C")).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.PrecioVenta.ToString("C")).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                        }
                        
                        document.Add(table);
                        
                        document.Add(new iText.Layout.Element.Paragraph($"\nTOTAL PRODUCTOS: {productos.Count}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(12));
                        document.Add(new iText.Layout.Element.Paragraph($"Valor aproximado (al costo): {productos.Sum(p => p.PrecioCompra * p.Stock):C}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(12));
                    }
                }

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath),
                    Title = "Reporte de Inventario"
                });

                await DisplayAlert("✅ Éxito", "Reporte de inventario generado correctamente.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", $"No se pudo generar el PDF:\n{ex.Message}", "OK");
            }
        }
    }
}
