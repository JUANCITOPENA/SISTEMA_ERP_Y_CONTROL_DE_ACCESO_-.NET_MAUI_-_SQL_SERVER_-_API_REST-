using System;

namespace CRUD_LOGIN_MAUI.Models
{
    public class ResumenVenta
    {
        public string Agrupador { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public int TotalArticulos { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Costos { get; set; }
        public decimal Margen { get; set; }
        public string PorcentajeMargen { get; set; } = string.Empty;
    }
}
