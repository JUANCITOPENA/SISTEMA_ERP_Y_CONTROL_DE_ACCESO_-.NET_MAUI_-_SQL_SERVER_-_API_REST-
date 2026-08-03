using CRUD_LOGIN_MAUI.Models;        // Modelos del CRUD
using CRUD_LOGIN_MAUI.Services;      // Servicios de acceso a datos
using System.Collections.ObjectModel; // Listas dinámicas para la UI
//using QuestPDF.Fluent;                // Construcción fluida de PDFs
//using QuestPDF.Helpers;               // Colores y utilidades visuales
//using QuestPDF.Infrastructure;        // Interfaces base de QuestPDF
using System.IO;                      // Manejo de archivos y streams


namespace CRUD_LOGIN_MAUI.Views
{
    /// <summary>
    /// Página de reportes con filtros avanzados y generación de PDF profesional.
    /// </summary>
    public partial class ReportesPage : ContentPage
    {
        VentaService service = new VentaService();                     // Servicio para manejar ventas (BD)
        List<Venta> listaBase = new List<Venta>();                     // Lista original con todas las ventas
        ObservableCollection<Venta> listaMostrada = new ObservableCollection<Venta>(); // Lista que se muestra en la UI

        public ReportesPage()
        {
            InitializeComponent();
            // Asigna la colección observable al CollectionView para actualizaciones en tiempo real
            ListaVentas.ItemsSource = listaMostrada;
        }

        /// <summary>
        /// Se ejecuta al mostrar la página. Carga catálogos y datos iniciales.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarFiltros();
            OnFiltrar(null, null);
        }

        /// <summary>
        /// Carga los listados de Vendedores, Clientes y Productos en los Pickers.
        /// </summary>
        private async void CargarFiltros()
        {
            try
            {
                pickerVendedor.ItemsSource = await service.GetVendedoresAsync();   // Carga lista de vendedores
                pickerCliente.ItemsSource = await service.GetClientesAsync();       // Carga lista de clientes
                pickerProducto.ItemsSource = await service.GetProductosAsync();     // Carga lista de productos
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta la consulta filtrada a la base de datos según los Pickers seleccionados.
        /// </summary>
        private async void OnFiltrar(object? sender, EventArgs? e)
        {
            try
            {
                int? cId = (pickerCliente.SelectedItem as Cliente)?.Id;      // Id del cliente seleccionado
                int? vId = (pickerVendedor.SelectedItem as Vendedor)?.Id;    // Id del vendedor seleccionado
                int? pId = (pickerProducto.SelectedItem as Producto)?.Id;    // Id del producto seleccionado

                listaBase = await service.GetReporteVentas(cId, vId, pId);   // Consulta filtrada a la BD
                AplicarBusquedaYResumen(searchBar.Text);                     // Aplica búsqueda y actualiza resumen
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Filtra la lista mostrada mientras el usuario escribe en el SearchBar.
        /// </summary>
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarBusquedaYResumen(e.NewTextValue);
        }

        /// <summary>
        /// Aplica filtro de texto y calcula el total general de las ventas visibles.
        /// </summary>
        /// <summary>
        /// Aplica un filtro de búsqueda sobre la lista ya filtrada por los Pickers,
        /// actualiza la lista mostrada en pantalla y recalcula el total general.
        /// Este método combina: búsqueda por texto, refresco visual y resumen monetario.
        /// </summary>
        private void AplicarBusquedaYResumen(string? texto)
        {
            var filtrado = string.IsNullOrWhiteSpace(texto) ? listaBase :
                listaBase.Where(v => v.ClienteNombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                                     v.ProductoNombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                                     v.VendedorNombre.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();

            listaMostrada.Clear();
            decimal total = 0;

            foreach (var v in filtrado)
            {
                listaMostrada.Add(v);
                total += v.Total;
            }

            lblTotalGeneral.Text = total.ToString("C");
        }


        /// <summary>
        /// Al seleccionar una venta en la lista, sincroniza los Pickers con los datos seleccionados.
        /// </summary>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var venta = e.CurrentSelection.FirstOrDefault() as Venta;                 // Obtiene la venta seleccionada
            if (venta != null)
            {
                if (pickerVendedor.ItemsSource is List<Vendedor> vends)               // Verifica lista de vendedores cargada
                    pickerVendedor.SelectedItem = vends.FirstOrDefault(x => x.Nombre == venta.VendedorNombre); // Selecciona vendedor correspondiente

                if (pickerCliente.ItemsSource is List<Cliente> clis)                  // Verifica lista de clientes cargada
                    pickerCliente.SelectedItem = clis.FirstOrDefault(x => x.Nombre == venta.ClienteNombre);    // Selecciona cliente correspondiente

                if (pickerProducto.ItemsSource is List<Producto> prods)               // Verifica lista de productos cargada
                    pickerProducto.SelectedItem = prods.FirstOrDefault(x => x.Nombre == venta.ProductoNombre); // Selecciona producto correspondiente
            }
        }


        /// <summary>
        /// Reinicia todos los filtros y recarga los datos sin restricciones.
        /// </summary>
        private void OnLimpiar(object sender, EventArgs e)
        {
            searchBar.Text = "";
            pickerCliente.SelectedIndex = pickerVendedor.SelectedIndex = pickerProducto.SelectedIndex = -1;
            OnFiltrar(null, null);
        }

        // ====================== GENERACIÓN DE PDF ======================

        /// <summary>
        /// Genera y abre un archivo PDF del reporte actual usando QuestPDF.
        /// Incluye filtros aplicados, tabla detallada y total general.
        /// </summary>
        private async void OnGenerarPDF(object sender, EventArgs e)
        {
            if (listaMostrada.Count == 0)                                           // Verifica si hay datos para generar PDF
            {
                await DisplayAlert("Sin datos", "No hay ventas para generar el reporte.", "OK"); // Alerta si no hay ventas
                return;                                                             // Sale del método
            }

            try
            {
                string fileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"; // Nombre del archivo PDF
                string filePath = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, fileName);    // Ruta donde se guardará el PDF

                using (var writer = new iText.Kernel.Pdf.PdfWriter(filePath))
                {
                    using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                    {
                        var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);
                        document.SetMargins(30, 30, 30, 30);

                        document.Add(new iText.Layout.Element.Paragraph("SUPERMARKET JPV")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(16));
                        document.Add(new iText.Layout.Element.Paragraph("REPORTE DE VENTAS")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(14));
                        document.Add(new iText.Layout.Element.Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        document.Add(new iText.Layout.Element.Paragraph("--------------------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        document.Add(new iText.Layout.Element.Paragraph("FILTROS APLICADOS:").SetFontSize(11));
                        document.Add(new iText.Layout.Element.Paragraph($"Cliente : {(pickerCliente.SelectedItem as Cliente)?.Nombre ?? "Todos"}").SetFontSize(10));
                        document.Add(new iText.Layout.Element.Paragraph($"Vendedor: {(pickerVendedor.SelectedItem as Vendedor)?.Nombre ?? "Todos"}").SetFontSize(10));
                        document.Add(new iText.Layout.Element.Paragraph($"Producto: {(pickerProducto.SelectedItem as Producto)?.Nombre ?? "Todos"}").SetFontSize(10));
                        document.Add(new iText.Layout.Element.Paragraph("--------------------------------------------------"));

                        iText.Layout.Element.Table table = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 15, 25, 25, 15, 10, 10 })).UseAllAvailableWidth();
                        
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("FECHA").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("CLIENTE").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRODUCTO").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("VENDEDOR").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("CANT").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("TOTAL").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));

                        foreach (var venta in listaMostrada)
                        {
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(venta.Fecha.ToString("dd/MM/yy")).SetFontSize(10)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(venta.ClienteNombre).SetFontSize(10)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(venta.ProductoNombre).SetFontSize(10)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(venta.VendedorNombre).SetFontSize(10)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(venta.Cantidad.ToString()).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(venta.Total.ToString("C")).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                        }
                        
                        document.Add(table);
                        
                        document.Add(new iText.Layout.Element.Paragraph($"\nTOTAL GENERAL: {listaMostrada.Sum(v => v.Total):C}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(14));
                    }
                }

                // Abre el PDF generado en el visor predeterminado
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath),                              // Archivo a abrir
                    Title = "Reporte de Ventas"                                     // Título del visor
                });

                await DisplayAlert("✅ Éxito", "Reporte PDF generado y abierto correctamente.", "OK"); // Mensaje de éxito
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", $"No se pudo generar el PDF:\n{ex.Message}", "OK"); // Manejo de errores
            }
        }


        
    }
}
