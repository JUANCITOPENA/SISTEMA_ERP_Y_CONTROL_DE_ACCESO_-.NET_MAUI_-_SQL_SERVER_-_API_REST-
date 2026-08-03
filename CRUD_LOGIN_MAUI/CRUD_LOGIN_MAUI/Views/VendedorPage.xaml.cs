using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using CRUD_LOGIN_MAUI.Models;
using CRUD_LOGIN_MAUI.Services;

namespace CRUD_LOGIN_MAUI.Views
{
    public partial class VendedorPage : ContentPage
    {
        private readonly VentaService _ventaService;
        public ObservableCollection<DetalleVenta> Carrito { get; set; } = new ObservableCollection<DetalleVenta>();
        private decimal _totalGeneral = 0;

        public VendedorPage()
        {
            InitializeComponent();
            _ventaService = new VentaService();
            listaCarrito.ItemsSource = Carrito;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarCatalogos();
        }

        private async System.Threading.Tasks.Task CargarCatalogos()
        {
            try
            {
                var clientes = await _ventaService.GetClientesAsync();
                pickerCliente.ItemsSource = clientes;

                var productos = await _ventaService.GetProductosAsync();
                pickerProducto.ItemsSource = productos;
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", "Error al cargar catálogos: " + ex.Message, "Entendido");
            }
        }

        private void OnAgregarClicked(object sender, EventArgs e)
        {
            if (pickerProducto.SelectedItem is not Producto prodSeleccionado)
            {
                DisplayAlert("⚠️ Atención", "Debe seleccionar un producto.", "OK");
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                DisplayAlert("⚠️ Atención", "Ingrese una cantidad válida mayor a 0.", "OK");
                return;
            }

            if (cantidad > prodSeleccionado.Stock)
            {
                DisplayAlert("⚠️ Atención", $"Solo hay {prodSeleccionado.Stock} unidades en stock.", "OK");
                return;
            }

            // Cálculos
            decimal subtotal = cantidad * prodSeleccionado.PrecioVenta;
            decimal itbis = subtotal * 0.18m;
            decimal total = subtotal + itbis;

            Carrito.Add(new DetalleVenta
            {
                ProductoId = prodSeleccionado.Id,
                ProductoNombre = prodSeleccionado.Nombre,
                Cantidad = cantidad,
                PrecioVentaAplicado = prodSeleccionado.PrecioVenta,
                SubTotal = subtotal,
                Itbis = itbis,
                Total = total
            });

            _totalGeneral += total;
            lblTotal.Text = _totalGeneral.ToString("C");

            // Limpiar inputs
            pickerProducto.SelectedIndex = -1;
            txtCantidad.Text = string.Empty;
        }

        private async void OnCobrarClicked(object sender, EventArgs e)
        {
            if (Carrito.Count == 0)
            {
                await DisplayAlert("⚠️ Atención", "El carrito está vacío.", "OK");
                return;
            }

            if (pickerCliente.SelectedItem is not Cliente clienteSeleccionado)
            {
                await DisplayAlert("⚠️ Atención", "Debe seleccionar un cliente.", "OK");
                return;
            }

            // NOTA: Para un sistema completo el VendedorId vendría de la sesión activa en MainPage.
            // Para fines de prueba usaremos el ID 1 de forma temporal.
            int vendedorId = 1; 

            try
            {
                int ventaId = await _ventaService.ProcesarVentaAsync(clienteSeleccionado.Id, vendedorId, Carrito.ToList());
                if (ventaId > 0)
                {
                    await DisplayAlert("✅ Éxito", "Venta procesada exitosamente.", "Excelente");
                    
                    // Generación del PDF con iText7 local
                    var pdfService = new TicketPdfService();
                    string rutaPdf = await pdfService.GenerarTicketPDFAsync(ventaId, clienteSeleccionado, "Vendedor " + vendedorId, Carrito.ToList(), _totalGeneral);
                    
                    // Mostrar alerta
                    await DisplayAlert("🖨️ Ticket Impreso", $"El ticket se generó con éxito.\nAbriendo documento...", "Genial");
                    
                    // Abrir el archivo PDF automáticamente para que el usuario lo vea
                    await Microsoft.Maui.ApplicationModel.Launcher.Default.OpenAsync(
                        new Microsoft.Maui.ApplicationModel.OpenFileRequest("Ver Ticket", new Microsoft.Maui.Storage.ReadOnlyFile(rutaPdf))
                    );

                    Carrito.Clear();
                    _totalGeneral = 0;
                    lblTotal.Text = "$0.00";
                    pickerCliente.SelectedIndex = -1;
                    
                    // Recargar stock de productos
                    var productos = await _ventaService.GetProductosAsync();
                    pickerProducto.ItemsSource = productos;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", "Ocurrió un error al procesar la venta: " + ex.Message, "OK");
            }
        }

        private async void OnVerReporteClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ResumenVentasPage");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("🔒 Confirmación", "¿Estás seguro de que deseas cerrar tu sesión?", "Sí, salir", "No, quedarme");
            if (answer)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
        }
    }
}
