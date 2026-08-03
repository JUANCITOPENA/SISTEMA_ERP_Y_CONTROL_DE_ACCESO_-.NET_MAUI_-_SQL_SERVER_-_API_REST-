using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using CRUD_LOGIN_MAUI.Models;

namespace CRUD_LOGIN_MAUI.Services
{
    public class VentaService
    {
        private readonly string _connectionString = ConfigDB.ConnectionString;

        public async Task<List<Cliente>> GetClientesAsync()
        {
            var lista = new List<Cliente>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT Id, Nombre, RNC, Telefono FROM Cliente ORDER BY Nombre", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Cliente
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    RNC = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
            return lista;
        }

        public async Task<List<Vendedor>> GetVendedoresAsync()
        {
            var lista = new List<Vendedor>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT Id, Nombre, Codigo FROM Vendedor ORDER BY Nombre", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Vendedor
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Codigo = reader.GetString(2)
                });
            }
            return lista;
        }

        public async Task<List<Producto>> GetProductosAsync()
        {
            var lista = new List<Producto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = @"
                SELECT p.Id, p.Nombre, p.CategoriaId, c.Nombre AS CategoriaNombre,
                       p.PrecioCompra, p.PrecioVenta, p.Stock
                FROM Producto p
                INNER JOIN Categoria c ON p.CategoriaId = c.Id
                WHERE p.Stock > 0
                ORDER BY p.Nombre";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Producto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    CategoriaId = reader.GetInt32(2),
                    CategoriaNombre = reader.GetString(3),
                    PrecioCompra = reader.GetDecimal(4),
                    PrecioVenta = reader.GetDecimal(5),
                    Stock = reader.GetInt32(6)
                });
            }
            return lista;
        }

        // Inserta la Venta y sus Detalles usando una Transacción SQL para asegurar integridad
        public async Task<List<Categoria>> GetCategorias(){ var list = new List<Categoria>(); using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString); await conn.OpenAsync(); var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT Id, Nombre FROM Categoria", conn); using var reader = await cmd.ExecuteReaderAsync(); while (await reader.ReadAsync()) list.Add(new Categoria { Id = (int)reader["Id"], Nombre = reader["Nombre"].ToString() ?? "" }); return list; } public async Task UpsertProducto(Producto p){ using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString); await conn.OpenAsync(); string query = p.Id == 0 ? "INSERT INTO Producto (Nombre, CategoriaId, PrecioCompra, PrecioVenta, Stock) VALUES (@N, @C, @PC, @PV, @S)" : "UPDATE Producto SET Nombre=@N, CategoriaId=@C, PrecioCompra=@PC, PrecioVenta=@PV, Stock=@S WHERE Id=@I"; var cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conn); if (p.Id > 0) cmd.Parameters.AddWithValue("@I", p.Id); cmd.Parameters.AddWithValue("@N", p.Nombre); cmd.Parameters.AddWithValue("@C", p.CategoriaId); cmd.Parameters.AddWithValue("@PC", p.PrecioCompra); cmd.Parameters.AddWithValue("@PV", p.PrecioVenta); cmd.Parameters.AddWithValue("@S", p.Stock); await cmd.ExecuteNonQueryAsync(); } 

        public async Task<(bool Exito, string Mensaje)> DeleteProductoAsync(int id)
        {
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Validar dependencias primero
            using var checkCmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM Detalle_Ventas WHERE ProductoId = @Id", conn);
            checkCmd.Parameters.AddWithValue("@Id", id);
            int count = (int)await checkCmd.ExecuteScalarAsync();
            
            if (count > 0)
            {
                return (false, "No se puede eliminar este producto porque ya tiene ventas (salidas) asociadas en el sistema.");
            }

            using var deleteCmd = new Microsoft.Data.SqlClient.SqlCommand("DELETE FROM Producto WHERE Id = @Id", conn);
            deleteCmd.Parameters.AddWithValue("@Id", id);
            await deleteCmd.ExecuteNonQueryAsync();
            
            return (true, "Producto eliminado correctamente.");
        }

        public async Task<List<Venta>> GetReporteVentas(int? clienteId, int? vendedorId, int? productoId){ var list = new List<Venta>(); using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString); await conn.OpenAsync(); string query = @"SELECT V.Id, V.Fecha, C.Nombre as ClienteNombre, Vend.Nombre as VendedorNombre, P.Nombre as ProductoNombre, DV.Cantidad, DV.PrecioVentaAplicado as Total FROM Ventas V INNER JOIN Cliente C ON V.ClienteId = C.Id INNER JOIN Vendedor Vend ON V.VendedorId = Vend.Id INNER JOIN Detalle_Ventas DV ON V.Id = DV.VentaId INNER JOIN Producto P ON DV.ProductoId = P.Id WHERE (@CID IS NULL OR V.ClienteId = @CID) AND (@VID IS NULL OR V.VendedorId = @VID) AND (@PID IS NULL OR DV.ProductoId = @PID) ORDER BY V.Fecha DESC"; var cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conn); cmd.Parameters.AddWithValue("@CID", (object)clienteId ?? DBNull.Value); cmd.Parameters.AddWithValue("@VID", (object)vendedorId ?? DBNull.Value); cmd.Parameters.AddWithValue("@PID", (object)productoId ?? DBNull.Value); using var reader = await cmd.ExecuteReaderAsync(); while (await reader.ReadAsync()){ list.Add(new Venta { Id = (int)reader["Id"], Fecha = (DateTime)reader["Fecha"], ClienteNombre = reader["ClienteNombre"].ToString() ?? "", VendedorNombre = reader["VendedorNombre"].ToString() ?? "", ProductoNombre = reader["ProductoNombre"].ToString() ?? "", Cantidad = (int)reader["Cantidad"], Total = (decimal)reader["Total"] * (int)reader["Cantidad"] }); } return list; } 

        public async Task<List<ResumenVenta>> GetResumenHistoricoAsync(DateTime fechaInicio, DateTime fechaFin, string agrupacion)
        {
            var list = new List<ResumenVenta>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            
            string agrupadorSql = "";
            string selectAgrupador = "";

            if (agrupacion == "Vendedor")
            {
                selectAgrupador = "Vend.Nombre";
                agrupadorSql = "Vend.Nombre";
            }
            else if (agrupacion == "Cliente")
            {
                selectAgrupador = "C.Nombre";
                agrupadorSql = "C.Nombre";
            }
            else if (agrupacion == "Producto")
            {
                selectAgrupador = "P.Nombre";
                agrupadorSql = "P.Nombre";
            }
            else // "General"
            {
                selectAgrupador = "'General'";
                agrupadorSql = "'General'";
            }

            string groupByClause = agrupacion != "General" ? $"GROUP BY {agrupadorSql}" : "";

            string query = $@"
                SELECT 
                    {selectAgrupador} as Agrupador,
                    COUNT(DISTINCT V.Id) as CantidadVentas,
                    SUM(DV.Cantidad) as TotalArticulos,
                    SUM(DV.Cantidad * DV.PrecioVentaAplicado) as Ingresos,
                    SUM(DV.Cantidad * P.PrecioCompra) as Costos
                FROM Ventas V
                INNER JOIN Cliente C ON V.ClienteId = C.Id
                INNER JOIN Vendedor Vend ON V.VendedorId = Vend.Id
                INNER JOIN Detalle_Ventas DV ON V.Id = DV.VentaId
                INNER JOIN Producto P ON DV.ProductoId = P.Id
                WHERE CAST(V.Fecha AS DATE) >= @Inicio AND CAST(V.Fecha AS DATE) <= @Fin
                {groupByClause}
                ORDER BY Ingresos DESC";

            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Inicio", fechaInicio.Date);
            cmd.Parameters.AddWithValue("@Fin", fechaFin.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                decimal ingresos = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                decimal costos = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                decimal margen = ingresos - costos;
                string porcentaje = ingresos > 0 ? ((margen / ingresos) * 100).ToString("F2") + "%" : "0.00%";

                list.Add(new ResumenVenta
                {
                    Agrupador = reader.GetString(0),
                    CantidadVentas = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    TotalArticulos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Ingresos = ingresos,
                    Costos = costos,
                    Margen = margen,
                    PorcentajeMargen = porcentaje
                });
            }
            return list;
        }

        public async Task<int> ProcesarVentaAsync(int clienteId, int vendedorId, List<DetalleVenta> carrito)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Insertar Cabecera (Ventas)
                string sqlVenta = @"INSERT INTO Ventas (ClienteId, VendedorId) 
                                    OUTPUT INSERTED.Id 
                                    VALUES (@ClienteId, @VendedorId);";
                using var cmdVenta = new SqlCommand(sqlVenta, conn, transaction);
                cmdVenta.Parameters.AddWithValue("@ClienteId", clienteId);
                cmdVenta.Parameters.AddWithValue("@VendedorId", vendedorId);

                int ventaId = Convert.ToInt32(await cmdVenta.ExecuteScalarAsync());

                // 2. Insertar Detalles (Detalle_Ventas) y Descontar Stock
                string sqlDetalle = @"INSERT INTO Detalle_Ventas (VentaId, ProductoId, Cantidad, PrecioVentaAplicado) 
                                      VALUES (@VentaId, @ProductoId, @Cantidad, @PrecioVentaAplicado);";
                string sqlStock = "UPDATE Producto SET Stock = Stock - @Cantidad WHERE Id = @ProductoId;";

                foreach (var item in carrito)
                {
                    // Insertar detalle
                    using var cmdDet = new SqlCommand(sqlDetalle, conn, transaction);
                    cmdDet.Parameters.AddWithValue("@VentaId", ventaId);
                    cmdDet.Parameters.AddWithValue("@ProductoId", item.ProductoId);
                    cmdDet.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                    cmdDet.Parameters.AddWithValue("@PrecioVentaAplicado", item.PrecioVentaAplicado);
                    await cmdDet.ExecuteNonQueryAsync();

                    // Descontar stock
                    using var cmdStock = new SqlCommand(sqlStock, conn, transaction);
                    cmdStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                    cmdStock.Parameters.AddWithValue("@ProductoId", item.ProductoId);
                    await cmdStock.ExecuteNonQueryAsync();
                }

                // Confirmar transacción
                transaction.Commit();
                return ventaId;
            }
            catch (Exception)
            {
                // Si algo falla, revertir todo (Rollback)
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetReporteAlmacen()
        {
            var list = new List<Dictionary<string, object>>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT ProductoId, Producto, Categoria, PrecioCompra, PrecioVenta, StockActual, TotalSalidas, StockInicialEntradas, ValorInventario FROM vw_ReporteInventario ORDER BY Producto", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var dic = new Dictionary<string, object>();
                dic["ProductoId"] = reader.GetInt32(0);
                dic["Producto"] = reader.GetString(1);
                dic["Categoria"] = reader.IsDBNull(2) ? "Sin Categoría" : reader.GetString(2);
                dic["PrecioCompra"] = reader.GetDecimal(3);
                dic["PrecioVenta"] = reader.GetDecimal(4);
                dic["StockActual"] = reader.GetInt32(5);
                dic["TotalSalidas"] = reader.GetInt32(6);
                dic["StockInicialEntradas"] = reader.GetInt32(7);
                dic["ValorInventario"] = reader.GetDecimal(8);
                list.Add(dic);
            }
            return list;
        }
    }
}
