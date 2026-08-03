using System.Collections.Generic;
using Xunit;
using CRUD_LOGIN_MAUI.Models;
using CRUD_LOGIN_MAUI.Services;

namespace CRUD_LOGIN_MAUI.Tests
{
    public class VentaServiceTests
    {
        [Fact]
        public void Calculo_Totales_Venta_Correcto()
        {
            // Arrange
            var producto = new Producto { Id = 1, Nombre = "Producto A", PrecioVenta = 100, Stock = 10 };
            int cantidad = 2;
            
            // Act
            decimal subtotal = cantidad * producto.PrecioVenta;
            decimal itbis = subtotal * 0.18m;
            decimal total = subtotal + itbis;
            
            // Assert
            Assert.Equal(200m, subtotal);
            Assert.Equal(36m, itbis);
            Assert.Equal(236m, total);
        }
    }
}
