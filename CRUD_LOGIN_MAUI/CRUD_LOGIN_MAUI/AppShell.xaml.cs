using Microsoft.Maui.Controls;
using CRUD_LOGIN_MAUI.Views;

namespace CRUD_LOGIN_MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Routing.RegisterRoute("AdminPage", typeof(AdminPage));
            Routing.RegisterRoute("SupervisorPage", typeof(SupervisorPage));
            Routing.RegisterRoute("VendedorPage", typeof(VendedorPage));
            Routing.RegisterRoute("AlmacenPage", typeof(AlmacenistaPage));
            Routing.RegisterRoute("RolesPage", typeof(RolesPage));
            Routing.RegisterRoute("InventarioPage", typeof(InventarioPage));
            Routing.RegisterRoute("ReportesPage", typeof(ReportesPage));
            Routing.RegisterRoute("ResumenVentasPage", typeof(ResumenVentasPage));
        }
    }
}
