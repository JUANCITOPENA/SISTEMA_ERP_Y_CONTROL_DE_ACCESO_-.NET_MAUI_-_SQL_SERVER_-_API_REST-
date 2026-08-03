using System.Collections.Generic;

namespace CRUD_LOGIN_MAUI.Api.Models
{
    public class TicketRequest
    {
        public int NumeroVenta { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string VendedorNombre { get; set; } = string.Empty;
        public decimal TotalGeneral { get; set; }
        public List<DetalleTicketRequest> Detalles { get; set; } = new();
    }

    public class DetalleTicketRequest
    {
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioVentaAplicado { get; set; }
        public decimal Total { get; set; }
    }
}
