using System;
using Microsoft.Maui.Controls;

namespace CRUD_LOGIN_MAUI.Views
{
    public partial class SupervisorPage : ContentPage
    {
        public SupervisorPage()
        {
            InitializeComponent();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("🔒 Confirmación", "¿Estás seguro de que deseas cerrar tu sesión?", "Sí, salir", "No, quedarme");
            if (answer)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
        }

        private async void OnInventarioClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("InventarioPage");
        }

        private async void OnReportesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ReportesPage");
        }

        private async void OnVerReporteClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("ResumenVentasPage");
        }
    }
}
