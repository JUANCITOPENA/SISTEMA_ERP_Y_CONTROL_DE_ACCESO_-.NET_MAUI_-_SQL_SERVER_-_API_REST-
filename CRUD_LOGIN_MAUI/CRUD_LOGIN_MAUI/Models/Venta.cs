using System;
using System.Collections.Generic;

namespace CRUD_LOGIN_MAUI.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public int VendedorId { get; set; }
        public string VendedorNombre { get; set; }
        
        // Atributo sumado desde los detalles
        public decimal TotalFactura { get; set; }

        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }

        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
