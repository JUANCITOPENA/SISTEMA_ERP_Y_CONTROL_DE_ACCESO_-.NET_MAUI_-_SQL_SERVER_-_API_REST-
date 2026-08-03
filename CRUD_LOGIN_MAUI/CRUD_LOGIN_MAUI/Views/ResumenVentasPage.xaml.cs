using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using CRUD_LOGIN_MAUI.Models;
using CRUD_LOGIN_MAUI.Services;

namespace CRUD_LOGIN_MAUI.Views
{
    public partial class ResumenVentasPage : ContentPage
    {
        private VentaService _ventaService = new VentaService();

        public ResumenVentasPage()
        {
            InitializeComponent();
            pickerAgrupacion.SelectedIndex = 0; // Por defecto "General"
            pickerRangoRapido.SelectedIndex = 2; // Por defecto "Este Mes"
        }

        private void OnRangoRapidoChanged(object sender, EventArgs e)
        {
            DateTime hoy = DateTime.Today;
            
            switch (pickerRangoRapido.SelectedItem?.ToString())
            {
                case "Hoy":
                    dpInicio.Date = hoy;
                    dpFin.Date = hoy;
                    break;
                case "Esta Semana":
                    int diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                    dpInicio.Date = hoy.AddDays(-1 * diff).Date;
                    dpFin.Date = dpInicio.Date.AddDays(6);
                    break;
                case "Este Mes":
                    dpInicio.Date = new DateTime(hoy.Year, hoy.Month, 1);
                    dpFin.Date = dpInicio.Date.AddMonths(1).AddDays(-1);
                    break;
                case "Este Año":
                    dpInicio.Date = new DateTime(hoy.Year, 1, 1);
                    dpFin.Date = new DateTime(hoy.Year, 12, 31);
                    break;
            }
        }

        private async void OnGenerarClicked(object sender, EventArgs e)
        {
            try
            {
                string agrupacion = pickerAgrupacion.SelectedItem?.ToString() ?? "General";
                
                var resumen = await _ventaService.GetResumenHistoricoAsync(dpInicio.Date, dpFin.Date, agrupacion);
                
                listaResumen.ItemsSource = resumen;

                // Calcular totales generales
                decimal totIngresos = resumen.Sum(r => r.Ingresos);
                decimal totCostos = resumen.Sum(r => r.Costos);
                decimal totMargen = resumen.Sum(r => r.Margen);

                lblTotalIngresos.Text = totIngresos.ToString("C");
                lblTotalCostos.Text = totCostos.ToString("C");
                lblTotalMargen.Text = totMargen.ToString("C");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo generar el resumen: {ex.Message}", "OK");
            }
        }
    }
}
