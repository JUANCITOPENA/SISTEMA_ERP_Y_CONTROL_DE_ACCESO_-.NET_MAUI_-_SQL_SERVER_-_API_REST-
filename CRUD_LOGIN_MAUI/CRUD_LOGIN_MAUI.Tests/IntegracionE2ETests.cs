using System;
using System.Threading.Tasks;
using Xunit;
using CRUD_LOGIN_MAUI.Services;
using Microsoft.Data.SqlClient;

namespace CRUD_LOGIN_MAUI.Tests
{
    public class IntegracionE2ETests
    {
        [Fact]
        public async Task Conexion_BaseDatos_Exitosa()
        {
            // Arrange
            string connectionString = ConfigDB.ConnectionString;
            bool conexionAbierta = false;
            
            // Act
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                conexionAbierta = conn.State == System.Data.ConnectionState.Open;
            }
            catch(Exception)
            {
                conexionAbierta = false;
            }

            // Assert
            Assert.True(conexionAbierta, "La conexión E2E a la base de datos SQL Server falló. Verifica la IP 10.0.0.15 y credenciales.");
        }
    }
}
