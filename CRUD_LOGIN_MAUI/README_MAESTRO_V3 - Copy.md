# 📘 BIBLIA DE INGENIERÍA JPV PRO V2.0: SISTEMA ERP Y CONTROL DE ACCESO (.NET MAUI + SQL SERVER + API REST)

Bienvenido al manual maestro y definitivo. Este documento contiene **ABSOLUTAMENTE TODO** el código, la arquitectura, la estructura del proyecto y las explicaciones pedagógicas de cada módulo para que el estudiante pueda replicarlo, entenderlo y llevarlo a producción.

---

## 📂 ESTRUCTURA DEL PROYECTO

Nuestra solución utiliza una arquitectura limpia separada en capas (MVC/MVVM guiado), compuesta por los siguientes proyectos:

`ext

1. **CRUD_LOGIN_MAUI**: La aplicación cliente multiplataforma.
2. **CRUD_LOGIN_MAUI.Api**: El backend (API REST) encargado de servicios pesados como la generación de PDFs.
3. **CRUD_LOGIN_MAUI.Tests**: Pruebas unitarias e integrales para garantizar la estabilidad.

---

## 📦 Árbol de la solución

CRUD_LOGIN_MAUI/
│
├── CRUD_LOGIN_MAUI.Api/               <-- Backend (.NET Core Web API)
│   ├── Program.cs
│   ├── appsettings.json
│   ├── CRUD_LOGIN_MAUI.Api.http
│   ├── Controllers/
│   │   └── PdfController.cs
│   ├── Models/
│   │   └── TicketRequest.cs
│   └── Services/
│       └── TicketPdfGenerator.cs
│
├── CRUD_LOGIN_MAUI/                   <-- Frontend (.NET MAUI)
│   ├── App.xaml / App.xaml.cs
│   ├── AppShell.xaml / AppShell.xaml.cs
│   ├── MauiProgram.cs
│   ├── Models/                        <-- Entidades de Datos
│   │   ├── Categoria.cs
│   │   ├── Cliente.cs
│   │   ├── DetalleVenta.cs
│   │   ├── Producto.cs
│   │   ├── ResumenVenta.cs
│   │   ├── Rol.cs
│   │   ├── Usuario.cs
│   │   ├── Vendedor.cs
│   │   └── Venta.cs
│   ├── Services/                      <-- Lógica de Negocio y Conexiones
│   │   ├── ConfigDB.cs                <-- Centralización de la Cadena de Conexión
│   │   ├── VentaService.cs
│   │   └── TicketPdfService.cs
│   └── Views/                         <-- Interfaces de Usuario (XAML + CS)
│       ├── MainPage.xaml / MainPage.xaml.cs          <-- Login
│       ├── AdminPage.xaml / AdminPage.xaml.cs        <-- Panel de Administrador
│       ├── AlmacenistaPage.xaml / AlmacenistaPage.xaml.cs  <-- Dashboard de Almacén
│       ├── InventarioPage.xaml / InventarioPage.xaml.cs    <-- CRUD de Productos
│       ├── ReportesPage.xaml / ReportesPage.xaml.cs        <-- Reportes Generales
│       ├── ResumenVentasPage.xaml / ResumenVentasPage.xaml.cs <-- Resumen de Ventas
│       ├── RolesPage.xaml / RolesPage.xaml.cs              <-- Gestión de Roles
│       ├── SupervisorPage.xaml / SupervisorPage.xaml.cs    <-- Panel de Supervisor
│       └── VendedorPage.xaml / VendedorPage.xaml.cs        <-- Panel de Vendedor
│
└── CRUD_LOGIN_MAUI.Tests/             <-- Pruebas (xUnit)
    ├── IntegracionE2ETests.cs         <-- Pruebas de Integración End-to-End
    └── VentaServiceTests.cs           <-- Pruebas Unitarias del Servicio de Ventas

`

---

## 🗄️ PASO 1: BASE DE DATOS Y CONEXIÓN

Antes de tocar el código C#, necesitamos la estructura de datos. Aquí crearemos las tablas relacionales para Usuarios, Roles, Productos, Categorías y Ventas.

### 📜 Script SQL Completo (Ejecutar en SQL Server)

`sql
-- Crear Base de Datos
CREATE DATABASE LoginRolesDB_cif;
GO
USE LoginRolesDB_cif;
GO

-- ==========================================
-- ESTRUCTURA DE SEGURIDAD (ROLES Y USUARIOS)
-- ==========================================
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NombreRol VARCHAR(50) NOT NULL
);

INSERT INTO Roles (NombreRol) VALUES ('Administrador'), ('Supervisor'), ('Vendedor'), ('Almacenista');

CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Usuario VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(64) NOT NULL, -- SHA2_256 (64 caracteres)
    IdRol INT NOT NULL,
    FOREIGN KEY (IdRol) REFERENCES Roles(Id)
);

-- HASHBYTES encripta la contraseña desde la BD
INSERT INTO Usuarios (Usuario, Password, IdRol) VALUES
('AdminUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), 1),
('SuperUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'super123'), 2), 2),
('SalesUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'sales123'), 2), 3),
('AlmUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2), 4);
GO

-- ==========================================
-- ESTRUCTURA ERP: PRODUCTOS, VENTAS E INVENTARIO
-- ==========================================
CREATE TABLE Categoria (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100) NOT NULL
);

CREATE TABLE Producto (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(150) NOT NULL,
    CategoriaId INT FOREIGN KEY REFERENCES Categoria(Id),
    PrecioCompra DECIMAL(10,2) NOT NULL,
    PrecioVenta DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL
);

CREATE TABLE Ventas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Fecha DATETIME DEFAULT GETDATE(),
    ClienteId INT NULL,
    VendedorId INT NULL
);

-- Regla de Negocio: PrecioVentaAplicado es un histórico inmutable
CREATE TABLE Detalle_Ventas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    VentaId INT FOREIGN KEY REFERENCES Ventas(Id),
    ProductoId INT FOREIGN KEY REFERENCES Producto(Id),
    Cantidad INT NOT NULL,
    PrecioVentaAplicado DECIMAL(10,2) NOT NULL 
);
GO

CREATE VIEW vw_ReporteInventario AS
SELECT 
    P.Id,
    P.Nombre,
    C.Nombre AS Categoria,
    P.PrecioCompra,
    P.PrecioVenta,
    P.Stock AS StockActual,
    ISNULL(SUM(DV.Cantidad), 0) AS TotalSalidas,
    (P.Stock + ISNULL(SUM(DV.Cantidad), 0)) AS StockInicialEntradas,
    (P.Stock * P.PrecioCompra) AS ValorTotalInventario
FROM Producto P
INNER JOIN Categoria C ON P.CategoriaId = C.Id
LEFT JOIN Detalle_Ventas DV ON P.Id = DV.ProductoId
GROUP BY P.Id, P.Nombre, C.Nombre, P.PrecioCompra, P.PrecioVenta, P.Stock;
GO
`

---

## ⚙️ PASO 2: CAPA DE SERVICIOS (Conexión Centralizada)

El error más común de los principiantes es poner la cadena de conexión (ConnectionString) regada por todas las ventanas. Nosotros utilizamos un patrón centralizado en Services/ConfigDB.cs.

### 📄 Services/ConfigDB.cs
`csharp
namespace CRUD_LOGIN_MAUI.Services
{
    public static class ConfigDB
    {
        public static string ConnectionString =>
            "Server=10.0.0.15,1433;Database=LoginRolesDB_cif_MINI_ERP;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";
    }
}
`

### 📄 Services/VentaService.cs
La lógica de base de datos para ventas e inventarios se encuentra aquí, aislando las consultas de la interfaz gráfica.
`csharp
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
`

### 📄 Services/TicketPdfService.cs
Este servicio se comunica con nuestra API REST para generar PDFs, en vez de sobrecargar la app móvil.
`csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CRUD_LOGIN_MAUI.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;

namespace CRUD_LOGIN_MAUI.Services
{
    public class TicketPdfService
    {
        public async Task<string> GenerarTicketPDFAsync(int numeroVenta, Cliente cliente, string vendedorNombre, List<DetalleVenta> carrito, decimal totalGeneral)
        {
            return await Task.Run(() =>
            {
                // Definir la ruta local en el dispositivo dependiendo del OS
                string fileName = $"Ticket_{numeroVenta}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                // FileSystem.CacheDirectory es ideal en MAUI para guardar archivos temporales como PDFs generados y abrirlos.
                string rutaPdf = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, fileName);

                using (var writer = new PdfWriter(rutaPdf))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        // Ancho de ticket térmico estándar de 80mm (~226 puntos de ancho)
                        PageSize rollSize = new PageSize(226, 800);
                        using (var document = new Document(pdf, rollSize))
                        {
                            // Márgenes reducidos para ticket térmico
                            document.SetMargins(10, 10, 10, 10);

                            // Encabezado
                            document.Add(new Paragraph("JPV PRO V2.0")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(14));

                            document.Add(new Paragraph("RNC: 101-12345-6")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(10));
                            
                            document.Add(new Paragraph("Av. Principal #123")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(10));
                                
                            document.Add(new Paragraph("Tel: 829-555-0000")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(10));

                            document.Add(new Paragraph("----------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            
                            // Información general
                            document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}").SetFontSize(10));
                            document.Add(new Paragraph($"Ticket: #{numeroVenta:D5}").SetFontSize(10));
                            document.Add(new Paragraph($"Cliente: {cliente.Nombre}").SetFontSize(10));
                            document.Add(new Paragraph($"Vendedor: {vendedorNombre}").SetFontSize(10));
                            
                            document.Add(new Paragraph("----------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            
                            // Tabla de Detalles
                            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 55, 15, 30 })).UseAllAvailableWidth();
                            
                            // Cabeceras de tabla
                            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("DESCRIPCIÓN").SetFontSize(10)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("CANT").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new Paragraph("TOT").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            
                            // Lista de productos iterados del carrito
                            foreach (var item in carrito)
                            {
                                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(item.ProductoNombre).SetFontSize(10)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph($"x{item.Cantidad}").SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                                table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(item.Total.ToString("C")).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            }
                            
                            document.Add(table);
                            
                            document.Add(new Paragraph("----------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            
                            // Cálculos finales: SubTotal, ITBIS, TOTAL
                            // Dado que SQL calcula Itbis como Subtotal * 0.18 y Total como SubTotal * 1.18
                            decimal subTotal = totalGeneral / 1.18m;
                            decimal itbis = totalGeneral - subTotal;

                            // Total General
                            document.Add(new Paragraph($"SubTotal: {subTotal:C}")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetFontSize(10));
                                
                            document.Add(new Paragraph($"ITBIS 18%: {itbis:C}")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetFontSize(10));
                                
                            document.Add(new Paragraph($"TOTAL: {totalGeneral:C}")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetFontSize(12));
                            
                            document.Add(new Paragraph("----------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            
                            Paragraph footer = new Paragraph("¡GRACIAS POR SU\nCOMPRA!")
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(10);
                            document.Add(footer);
                        }
                    }
                }
                
                return rutaPdf;
            });
        }
    }
}
`

---

## 🚀 PASO 3: EL BACKEND (API REST PARA PDF)

Nuestra aplicación delega el trabajo pesado (como crear PDFs) a una API en ASP.NET Core.

### 📄 CRUD_LOGIN_MAUI.Api / Program.cs
`csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();
`

### 📄 CRUD_LOGIN_MAUI.Api / Controllers / PdfController.cs
`csharp
using Microsoft.AspNetCore.Mvc;
using CRUD_LOGIN_MAUI.Api.Models;
using CRUD_LOGIN_MAUI.Api.Services;

namespace CRUD_LOGIN_MAUI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        [HttpPost("ticket")]
        public IActionResult GenerarTicket([FromBody] TicketRequest request)
        {
            var generator = new TicketPdfGenerator();
            var pdfBytes = generator.GenerarPdf(request);
            return File(pdfBytes, "application/pdf", $"Ticket_{request.NumeroVenta}.pdf");
        }
    }
}
`

### 📄 CRUD_LOGIN_MAUI.Api / Services / TicketPdfGenerator.cs
Usamos iText7 para dibujar el PDF exacto (factura térmica 80mm).
`csharp
using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CRUD_LOGIN_MAUI.Api.Models;

namespace CRUD_LOGIN_MAUI.Api.Services
{
    public class TicketPdfGenerator
    {
        public byte[] GenerarPdf(TicketRequest request)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(5, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, request));
                    page.Footer().AlignCenter().Text("¡Gracias por su compra!").SemiBold();
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("SUPERMERCADO JPV").FontSize(14).SemiBold();
                column.Item().AlignCenter().Text("RNC: 101-23456-7");
                column.Item().AlignCenter().Text("Av. Principal #123, SD");
                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
            });
        }

        private void ComposeContent(IContainer container, TicketRequest request)
        {
            container.Column(column =>
            {
                column.Item().Text($"Ticket #: {request.NumeroVenta}");
                column.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
                column.Item().Text($"Cliente: {request.ClienteNombre}");
                column.Item().Text($"Cajero: {request.VendedorNombre}");
                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);

                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Text("Desc").SemiBold();
                    row.RelativeItem(1).AlignRight().Text("Cant").SemiBold();
                    row.RelativeItem(2).AlignRight().Text("Prec").SemiBold();
                    row.RelativeItem(2).AlignRight().Text("Total").SemiBold();
                });

                foreach (var item in request.Detalles)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text(item.ProductoNombre);
                        row.RelativeItem(1).AlignRight().Text(item.Cantidad.ToString());
                        row.RelativeItem(2).AlignRight().Text(item.PrecioVentaAplicado.ToString("C"));
                        row.RelativeItem(2).AlignRight().Text(item.Total.ToString("C"));
                    });
                }

                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                column.Item().AlignRight().Text($"TOTAL: {request.TotalGeneral:C}").FontSize(12).SemiBold();
            });
        }
    }
}
`

---

## 🎨 PASO 4: INTERFACES GRÁFICAS (VISTAS MAUI)

A continuación, el código completo de todas nuestras pantallas (XAML y C#). 

### 4.1 Configuración de Rutas (AppShell)
Para navegar entre pantallas, registramos las rutas en el Shell.

#### 📄 AppShell.xaml
`xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="CRUD_LOGIN_MAUI.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:CRUD_LOGIN_MAUI.Views"
    Title="Mini ERP JPV"
    FlyoutBehavior="Disabled">

    <ShellContent
        Title="Login"
        ContentTemplate="{DataTemplate views:MainPage}"
        Route="MainPage" />
</Shell>
`

#### 📄 AppShell.xaml.cs
`csharp
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
`

### 4.2 Pantalla de Login (MainPage)
Controla el acceso y redirige según el Rol del usuario utilizando consultas seguras a ConfigDB.

#### 📄 Views/MainPage.xaml
`xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.Views.MainPage">

    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#F5F5F5">

        <Image Source="https://images.icon-icons.com/2120/PNG/512/lock_padlock_locked_protected_security_icon_131240.png"
               HeightRequest="175" HorizontalOptions="Center" />

        <Entry x:Name="txtUsuario"
               Placeholder="Usuario"
               ClearButtonVisibility="WhileEditing"
               HorizontalTextAlignment="Center"
               TextColor="Black"
               FontSize="25"
               Margin="10"/>

        <!-- Grid para Contraseña + Botón Ojito -->
        <Grid Margin="10">
            <Entry x:Name="txtPassword"
                   Placeholder="Contraseña"
                   IsPassword="True"
                   HorizontalTextAlignment="Center"
                   TextColor="Black"
                   FontSize="25"/>

            <Button x:Name="btnTogglePassword"
                    Text="👁️"
                    Clicked="OnTogglePasswordClicked"
                    BackgroundColor="Transparent"
                    HorizontalOptions="End"
                    WidthRequest="60"/>
        </Grid>

        <Button Text="Ingresar"
                BackgroundColor="#fbc531"
                TextColor="#40739e"
                FontAttributes="Bold"
                Margin="20"
                Clicked="OnLogin_Clicked"
                FontSize="25" />

        <Label x:Name="lblMensaje"
               TextColor="Red"
               FontSize="22"
               FontAttributes="Bold"
               HorizontalTextAlignment="Center" />

    </VerticalStackLayout>
</ContentPage>
`

#### 📄 Views/MainPage.xaml.cs
`csharp
using CRUD_LOGIN_MAUI.Models;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Controls;
using CRUD_LOGIN_MAUI.Services;

namespace CRUD_LOGIN_MAUI.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            txtUsuario.Text = string.Empty;
            txtPassword.Text = string.Empty;
            lblMensaje.IsVisible = false;
        }

        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            txtPassword.IsPassword = !txtPassword.IsPassword;
        }

        private async void OnLogin_Clicked(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text?.Trim();
            string password = txtPassword.Text?.Trim();

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                lblMensaje.Text = "❌ Ingrese sus credenciales.";
                lblMensaje.IsVisible = true;
                return;
            }

            try
            {
                // Usando ConfigDB en lugar de la cadena harcodeada
                using (var connection = new SqlConnection(ConfigDB.ConnectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT R.NombreRol 
                                     FROM Usuarios U 
                                     INNER JOIN Roles R ON U.IdRol = R.Id 
                                     WHERE U.Usuario = @Usuario 
                                     AND U.Password = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                        command.Parameters.Add("@Password", SqlDbType.VarChar, 50).Value = password;

                        var roleResult = await command.ExecuteScalarAsync();

                        if (roleResult != null)
                        {
                            string role = roleResult.ToString();
                            // Navegar a la página correspondiente (AdminPage, SupervisorPage o VendedorPage)
                            await Shell.Current.GoToAsync($"{role}Page");
                        }
                        else
                        {
                            lblMensaje.Text = "❌ Usuario o contraseña incorrectos.";
                            lblMensaje.IsVisible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"❌ Error de conexión: {ex.Message}";
                lblMensaje.IsVisible = true;
            }
        }
    }
}
`

### 4.3 Panel de Inventario (InventarioPage)
CRUD completo de productos con buscador en tiempo real y protección de integridad referencial.

#### 📄 Views/InventarioPage.xaml
`xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:CRUD_LOGIN_MAUI.Models"
             x:Class="CRUD_LOGIN_MAUI.Views.InventarioPage"
             BackgroundColor="White">
    <Grid RowDefinitions="Auto, *" Padding="20">

        <VerticalStackLayout Grid.Row="0" Spacing="8" Margin="0,0,0,20">
            <Label Text="REGISTRO DE PRODUCTOS" FontSize="18" FontAttributes="Bold" HorizontalOptions="Center" TextColor="Black"/>
            <Entry x:Name="txtNombre" Placeholder="Nombre del Producto" TextColor="Black" BackgroundColor="#F0F0F0" FontAttributes="Bold" HeightRequest="45"/>

            <Label Text="CATEGORÍA" FontSize="12" TextColor="Black" FontAttributes="Bold" Margin="5,0"/>
            <Picker x:Name="pickerCategoria" Title="-- Seleccione --" ItemDisplayBinding="{Binding Nombre}"
                    BackgroundColor="#F0F0F0" TextColor="Black" HeightRequest="70" TitleColor="Black" FontAttributes="Bold"/>
            <Grid ColumnDefinitions="*, *" ColumnSpacing="15">
                <VerticalStackLayout Grid.Column="0">
                    <Label Text="COSTO" FontSize="12" TextColor="Black" FontAttributes="Bold"/>
                    <Entry x:Name="txtPrecioCompra" Placeholder="0.00" Keyboard="Numeric" TextColor="Black" BackgroundColor="#F0F0F0" FontAttributes="Bold" HeightRequest="45"/>
                </VerticalStackLayout>
                <VerticalStackLayout Grid.Column="1">
                    <Label Text="VENTA" FontSize="12" TextColor="Black" FontAttributes="Bold"/>
                    <Entry x:Name="txtPrecioVenta" Placeholder="0.00" Keyboard="Numeric" TextColor="Black" BackgroundColor="#F0F0F0" FontAttributes="Bold" HeightRequest="45"/>
                </VerticalStackLayout>
            </Grid>

            <Label Text="STOCK" FontSize="12" TextColor="Black" FontAttributes="Bold" Margin="5,0"/>
            <Entry x:Name="txtStock" Placeholder="0" Keyboard="Numeric" TextColor="Black" BackgroundColor="#F0F0F0" FontAttributes="Bold" HeightRequest="45"/>
            <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto" ColumnSpacing="15" RowSpacing="10" Margin="0,10">
                <Button Grid.Row="0" Grid.Column="0" Text="GUARDAR" Clicked="OnGuardar" BackgroundColor="Green" TextColor="White" FontAttributes="Bold" CornerRadius="8"/>
                <Button Grid.Row="0" Grid.Column="1" Text="LIMPIAR" Clicked="OnLimpiar" BackgroundColor="Gray" TextColor="White" FontAttributes="Bold" CornerRadius="8"/>
                <Button Grid.Row="1" Grid.Column="0" Text="ELIMINAR" Clicked="OnEliminar" BackgroundColor="Red" TextColor="White" FontAttributes="Bold" CornerRadius="8"/>
                <Button Grid.Row="1" Grid.Column="1" Text="GENERAR PDF" Clicked="OnGenerarPDF" BackgroundColor="DarkBlue" TextColor="White" FontAttributes="Bold" CornerRadius="8"/>
            </Grid>
        </VerticalStackLayout>
        
        <Grid Grid.Row="1" RowDefinitions="Auto, *">
            <!-- SearchBar para filtros dinámicos -->
            <SearchBar x:Name="searchBar" Grid.Row="0" Placeholder="Buscar producto por nombre o categoría..." 
                       TextChanged="OnSearchTextChanged" BackgroundColor="#F0F0F0" TextColor="Black" CancelButtonColor="Red" Margin="0,0,0,10" />

            <CollectionView Grid.Row="1" x:Name="ListaProductos" SelectionMode="Single" SelectionChanged="OnSelectionChanged">
                <CollectionView.ItemsLayout>
                <LinearItemsLayout Orientation="Vertical" ItemSpacing="10" />
            </CollectionView.ItemsLayout>
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:Producto">
                    <Frame Margin="0,5" Padding="20" BorderColor="Black" BackgroundColor="White" HasShadow="True" CornerRadius="10" InputTransparent="True">
                        <Grid ColumnDefinitions="*, Auto">
                            <VerticalStackLayout Grid.Column="0" Spacing="4">
                                <Label Text="{Binding Nombre}" FontAttributes="Bold" FontSize="17" TextColor="Black"/>
                                <Label Text="{Binding CategoriaNombre, StringFormat='Categoría: {0}'}" FontSize="13" FontAttributes="Bold" TextColor="#333333"/>
                                <Label Text="{Binding Stock, StringFormat='Existencia: {0}'}" TextColor="#0056B3" FontAttributes="Bold" FontSize="14"/>
                            </VerticalStackLayout>
                            <VerticalStackLayout Grid.Column="1" VerticalOptions="Center">
                                <Label Text="{Binding PrecioVenta, StringFormat='{0:C}'}" FontAttributes="Bold" TextColor="DarkGreen" FontSize="18" HorizontalOptions="End"/>
                                <Label Text="{Binding PrecioCompra, StringFormat='Costo: {0:C}'}" FontSize="11" FontAttributes="Bold" TextColor="DarkRed" HorizontalOptions="End"/>
                            </VerticalStackLayout>
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        </Grid>
    </Grid>
</ContentPage>
`

#### 📄 Views/InventarioPage.xaml.cs
`csharp
using CRUD_LOGIN_MAUI.Models;
using CRUD_LOGIN_MAUI.Services;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;
using System.IO;

namespace CRUD_LOGIN_MAUI.Views
{
    public partial class InventarioPage : ContentPage
    {
        VentaService service = new VentaService();
        Producto? productoSeleccionado;

        public InventarioPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarCategorias();
            CargarProductos();
        }

        private async void CargarCategorias()
        {
            pickerCategoria.ItemsSource = await service.GetCategorias();
        }

        private async void CargarProductos()
        {
            ListaProductos.ItemsSource = await service.GetProductosAsync();
        }

        private async void OnGuardar(object sender, EventArgs e)
        {
            try
            {
                var cat = pickerCategoria.SelectedItem as Categoria;
                if (cat == null || string.IsNullOrEmpty(txtNombre.Text)) return;
                var p = productoSeleccionado ?? new Producto();
                p.Nombre = txtNombre.Text;
                p.CategoriaId = cat.Id;
                p.PrecioCompra = decimal.Parse(txtPrecioCompra.Text);
                p.PrecioVenta = decimal.Parse(txtPrecioVenta.Text);
                p.Stock = int.Parse(txtStock.Text);
                await service.UpsertProducto(p);
                OnLimpiar(null, null);
                CargarProductos();
                await DisplayAlert("Éxito", "Producto guardado.", "OK");
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                productoSeleccionado = e.CurrentSelection.FirstOrDefault() as Producto;
                if (productoSeleccionado != null)
                {
                    txtNombre.Text = productoSeleccionado.Nombre;
                    txtPrecioCompra.Text = productoSeleccionado.PrecioCompra.ToString();
                    txtPrecioVenta.Text = productoSeleccionado.PrecioVenta.ToString();
                    txtStock.Text = productoSeleccionado.Stock.ToString();

                    var categorias = pickerCategoria.ItemsSource as List<Categoria>;
                    if (categorias != null)
                    {
                        pickerCategoria.SelectedIndex = categorias.FindIndex(x => x.Id == productoSeleccionado.CategoriaId);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error en Selección", ex.Message, "OK");
            }
        }

        private void OnLimpiar(object? sender, EventArgs? e)
        {
            txtNombre.Text = txtPrecioCompra.Text = txtPrecioVenta.Text = txtStock.Text = "";
            pickerCategoria.SelectedIndex = -1;
            productoSeleccionado = null;
            ListaProductos.SelectedItem = null;
        }

        private async void OnEliminar(object sender, EventArgs e)
        {
            if (productoSeleccionado == null || productoSeleccionado.Id == 0)
            {
                await DisplayAlert("Atención", "Seleccione un producto de la lista para eliminar.", "OK");
                return;
            }

            bool answer = await DisplayAlert("Confirmar", $"¿Está seguro de eliminar '{productoSeleccionado.Nombre}'?\n\n(No se borrará si tiene ventas asociadas)", "Sí, eliminar", "No");
            if (answer)
            {
                try
                {
                    var result = await service.DeleteProductoAsync(productoSeleccionado.Id);
                    if (result.Exito)
                    {
                        await DisplayAlert("Éxito", result.Mensaje, "OK");
                        OnLimpiar(null, null);
                        CargarProductos();
                    }
                    else
                    {
                        await DisplayAlert("Restricción", result.Mensaje, "Entendido");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = e.NewTextValue?.ToLowerInvariant();
            var allProducts = await service.GetProductosAsync();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ListaProductos.ItemsSource = allProducts;
            }
            else
            {
                ListaProductos.ItemsSource = allProducts.Where(p => 
                    (p.Nombre != null && p.Nombre.ToLowerInvariant().Contains(keyword)) ||
                    (p.CategoriaNombre != null && p.CategoriaNombre.ToLowerInvariant().Contains(keyword))
                ).ToList();
            }
        }

        // ====================== GENERACIÓN DE REPORTE PDF ======================
        private async void OnGenerarPDF(object sender, EventArgs e)
        {
            try
            {
                var productos = await service.GetProductosAsync();
                if (productos == null || productos.Count == 0)
                {
                    await DisplayAlert("Sin datos", "No hay productos en el inventario.", "OK");
                    return;
                }

                string fileName = $"Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, fileName);

                using (var writer = new iText.Kernel.Pdf.PdfWriter(filePath))
                {
                    using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                    {
                        var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);
                        document.SetMargins(30, 30, 30, 30);

                        document.Add(new iText.Layout.Element.Paragraph("SUPERMARKET JPV")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(16));
                        document.Add(new iText.Layout.Element.Paragraph("REPORTE DE INVENTARIO")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(14));
                        document.Add(new iText.Layout.Element.Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        document.Add(new iText.Layout.Element.Paragraph("--------------------------------------------------").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        iText.Layout.Element.Table table = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 35, 25, 10, 15, 15 })).UseAllAvailableWidth();
                        
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRODUCTO").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("CATEGORIA").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("STOCK").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("COSTO").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRECIO V.").SetFontSize(10)));

                        foreach (var prod in productos)
                        {
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.Nombre).SetFontSize(10)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.CategoriaNombre ?? "Sin categoría").SetFontSize(10)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.Stock.ToString()).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.PrecioCompra.ToString("C")).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(prod.PrecioVenta.ToString("C")).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                        }
                        
                        document.Add(table);
                        
                        document.Add(new iText.Layout.Element.Paragraph($"\nTOTAL PRODUCTOS: {productos.Count}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(12));
                        document.Add(new iText.Layout.Element.Paragraph($"Valor aproximado (al costo): {productos.Sum(p => p.PrecioCompra * p.Stock):C}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(12));
                    }
                }

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath),
                    Title = "Reporte de Inventario"
                });

                await DisplayAlert("✅ Éxito", "Reporte de inventario generado correctamente.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", $"No se pudo generar el PDF:\n{ex.Message}", "OK");
            }
        }
    }
}
`

### 4.4 Dashboard del Almacenista (AlmacenistaPage)
Tarjetas KPIs mostrando Stock Inicial, Salidas y Stock Actual dinámicamente.

#### 📄 Views/AlmacenistaPage.xaml
`xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.Views.AlmacenistaPage"
             Title="Panel de Almacén"
             Shell.NavBarIsVisible="False"
             BackgroundColor="#F7F9FC">

    <Grid RowDefinitions="Auto, Auto, *" Padding="15" RowSpacing="15">
        
        <!-- HEADER -->
        <Grid Grid.Row="0" ColumnDefinitions="*, Auto, Auto, Auto" ColumnSpacing="10" Margin="0,5">
            <VerticalStackLayout Grid.Column="0" VerticalOptions="Center">
                <Label Text="📦 Dashboard Almacén" FontSize="22" FontAttributes="Bold" TextColor="#2C3E50"/>
                <Label Text="Control y Rotación de Inventario" FontSize="12" TextColor="#7F8C8D"/>
            </VerticalStackLayout>

            <Button Grid.Column="1" Text="➕ CRUD" Clicked="OnCrudClicked" BackgroundColor="#2980B9" TextColor="White" CornerRadius="8" HeightRequest="40" Padding="15,0" FontAttributes="Bold"/>
            <Button Grid.Column="2" Text="📄 PDF" Clicked="OnPdfClicked" BackgroundColor="#E67E22" TextColor="White" CornerRadius="8" HeightRequest="40" Padding="15,0" FontAttributes="Bold"/>
            <Button Grid.Column="3" Text="🚪 Salir" Clicked="OnLogoutClicked" BackgroundColor="#C0392B" TextColor="White" CornerRadius="8" HeightRequest="40" Padding="15,0" FontAttributes="Bold"/>
        </Grid>

        <!-- KPIS / RESUMEN -->
        <ScrollView Grid.Row="1" Orientation="Horizontal" HorizontalScrollBarVisibility="Never">
            <HorizontalStackLayout Spacing="15" Padding="5">
                <Border StrokeShape="RoundRectangle 12" Stroke="#E0E0E0" BackgroundColor="White" WidthRequest="160" Padding="15">
                    <VerticalStackLayout Spacing="5" VerticalOptions="Center">
                        <Label Text="Productos" FontSize="13" TextColor="#95A5A6" FontAttributes="Bold"/>
                        <Label x:Name="lblTotalProd" Text="0" FontSize="24" FontAttributes="Bold" TextColor="#2C3E50"/>
                    </VerticalStackLayout>
                </Border>
                <Border StrokeShape="RoundRectangle 12" Stroke="#E0E0E0" BackgroundColor="White" WidthRequest="160" Padding="15">
                    <VerticalStackLayout Spacing="5" VerticalOptions="Center">
                        <Label Text="Valor Total" FontSize="13" TextColor="#95A5A6" FontAttributes="Bold"/>
                        <Label x:Name="lblValorInv" Text="$0.00" FontSize="20" FontAttributes="Bold" TextColor="#27AE60"/>
                    </VerticalStackLayout>
                </Border>
                <Border StrokeShape="RoundRectangle 12" Stroke="#E0E0E0" BackgroundColor="White" WidthRequest="160" Padding="15">
                    <VerticalStackLayout Spacing="5" VerticalOptions="Center">
                        <Label Text="Entradas" FontSize="13" TextColor="#95A5A6" FontAttributes="Bold"/>
                        <Label x:Name="lblTotalEntradas" Text="0" FontSize="24" FontAttributes="Bold" TextColor="#2980B9"/>
                    </VerticalStackLayout>
                </Border>
                <Border StrokeShape="RoundRectangle 12" Stroke="#E0E0E0" BackgroundColor="White" WidthRequest="160" Padding="15">
                    <VerticalStackLayout Spacing="5" VerticalOptions="Center">
                        <Label Text="Salidas" FontSize="13" TextColor="#95A5A6" FontAttributes="Bold"/>
                        <Label x:Name="lblTotalSalidas" Text="0" FontSize="24" FontAttributes="Bold" TextColor="#C0392B"/>
                    </VerticalStackLayout>
                </Border>
            </HorizontalStackLayout>
        </ScrollView>

        <!-- DATA LIST (CARDS) -->
        <CollectionView x:Name="listaAlmacen" Grid.Row="2" Margin="0,10,0,0">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Border StrokeShape="RoundRectangle 10" Stroke="#E0E0E0" BackgroundColor="White" Margin="0,0,0,12" Padding="15">
                        <Grid RowDefinitions="Auto, Auto" ColumnDefinitions="*, Auto">
                            
                            <!-- Izquierda: Info Producto -->
                            <VerticalStackLayout Grid.Row="0" Grid.Column="0" Spacing="2">
                                <Label Text="{Binding [Producto]}" FontSize="16" FontAttributes="Bold" TextColor="#2C3E50"/>
                                <Label Text="{Binding [Categoria]}" FontSize="13" TextColor="#7F8C8D"/>
                            </VerticalStackLayout>
                            
                            <!-- Derecha: Valor -->
                            <VerticalStackLayout Grid.Row="0" Grid.Column="1" HorizontalOptions="End" VerticalOptions="Center">
                                <Label Text="Valor Actual" FontSize="11" TextColor="#95A5A6" HorizontalOptions="End"/>
                                <Label Text="{Binding [ValorInventario], StringFormat='{0:C}'}" FontSize="15" FontAttributes="Bold" TextColor="#27AE60" HorizontalOptions="End"/>
                            </VerticalStackLayout>

                            <!-- Abajo: Metricas (Entradas, Salidas, Stock) -->
                            <Grid Grid.Row="1" Grid.ColumnSpan="2" ColumnDefinitions="*,*,*" Margin="0,12,0,0">
                                <!-- Entradas -->
                                <VerticalStackLayout Grid.Column="0">
                                    <Label Text="ENTRADAS" FontSize="11" TextColor="#95A5A6" FontAttributes="Bold"/>
                                    <Label Text="{Binding [StockInicialEntradas]}" FontSize="16" FontAttributes="Bold" TextColor="#2980B9"/>
                                </VerticalStackLayout>
                                <!-- Salidas -->
                                <VerticalStackLayout Grid.Column="1" HorizontalOptions="Center">
                                    <Label Text="SALIDAS" FontSize="11" TextColor="#95A5A6" FontAttributes="Bold"/>
                                    <Label Text="{Binding [TotalSalidas]}" FontSize="16" FontAttributes="Bold" TextColor="#C0392B"/>
                                </VerticalStackLayout>
                                <!-- Stock -->
                                <VerticalStackLayout Grid.Column="2" HorizontalOptions="End">
                                    <Label Text="STOCK ACTUAL" FontSize="11" TextColor="#95A5A6" FontAttributes="Bold" HorizontalOptions="End"/>
                                    <Label Text="{Binding [StockActual]}" FontSize="16" FontAttributes="Bold" TextColor="#2C3E50" HorizontalOptions="End"/>
                                </VerticalStackLayout>
                            </Grid>
                        </Grid>
                    </Border>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

    </Grid>
</ContentPage>
`

#### 📄 Views/AlmacenistaPage.xaml.cs
`csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using CRUD_LOGIN_MAUI.Services;
using Microsoft.Maui.ApplicationModel;
using System.IO;

namespace CRUD_LOGIN_MAUI.Views
{
    public partial class AlmacenistaPage : ContentPage
    {
        private VentaService _service = new VentaService();
        private List<Dictionary<string, object>> _currentData = new List<Dictionary<string, object>>();

        public AlmacenistaPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                _currentData = await _service.GetReporteAlmacen();
                
                // Actualizar KPIs
                lblTotalProd.Text = _currentData.Count.ToString();
                
                decimal valorTotal = _currentData.Sum(d => Convert.ToDecimal(d["ValorInventario"]));
                lblValorInv.Text = valorTotal.ToString("C");
                
                int entradas = _currentData.Sum(d => Convert.ToInt32(d["StockInicialEntradas"]));
                lblTotalEntradas.Text = entradas.ToString("N0");
                
                int salidas = _currentData.Sum(d => Convert.ToInt32(d["TotalSalidas"]));
                lblTotalSalidas.Text = salidas.ToString("N0");

                // Asignar al CollectionView
                listaAlmacen.ItemsSource = _currentData;
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", "No se pudo cargar el reporte: " + ex.Message, "Entendido");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("🔒 Confirmación", "¿Estás seguro de que deseas cerrar tu sesión?", "Sí, salir", "No, quedarme");
            if (answer)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
        }

        private async void OnCrudClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("InventarioPage");
        }

        private async void OnPdfClicked(object sender, EventArgs e)
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                await DisplayAlert("⚠️ Vacío", "No hay datos para generar el reporte.", "OK");
                return;
            }

            try
            {
                string fileName = $"Reporte_Almacen_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                using (var writer = new iText.Kernel.Pdf.PdfWriter(filePath))
                {
                    using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                    {
                        var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);
                        document.SetMargins(30, 30, 30, 30);

                        // Cabecera
                        document.Add(new iText.Layout.Element.Paragraph("📦 SUPERMARKET JPV - ALMACÉN")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(16));
                        document.Add(new iText.Layout.Element.Paragraph("REPORTE DE ROTACIÓN DE INVENTARIO")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetFontSize(14));
                        document.Add(new iText.Layout.Element.Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        document.Add(new iText.Layout.Element.Paragraph("-------------------------------------------------------------------------")
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        // KPIs Globales
                        decimal totalValor = _currentData.Sum(d => Convert.ToDecimal(d["ValorInventario"]));
                        int totalEntradas = _currentData.Sum(d => Convert.ToInt32(d["StockInicialEntradas"]));
                        int totalSalidas = _currentData.Sum(d => Convert.ToInt32(d["TotalSalidas"]));

                        document.Add(new iText.Layout.Element.Paragraph($"TOTAL PRODUCTOS: {_currentData.Count} | VALOR INVENTARIO: {totalValor:C}").SetFontSize(12));
                        document.Add(new iText.Layout.Element.Paragraph($"TOTAL ENTRADAS: {totalEntradas} | TOTAL SALIDAS: {totalSalidas}").SetFontSize(12));
                        document.Add(new iText.Layout.Element.Paragraph("\n"));

                        // Tabla
                        iText.Layout.Element.Table table = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 30, 20, 12, 12, 12, 14 })).UseAllAvailableWidth();
                        
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRODUCTO").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("CATEGORÍA").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("ENTRADAS").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("SALIDAS").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("STOCK").SetFontSize(10)));
                        table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("VALOR").SetFontSize(10)));

                        foreach (var item in _currentData)
                        {
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["Producto"].ToString()).SetFontSize(9)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["Categoria"].ToString()).SetFontSize(9)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["StockInicialEntradas"].ToString()).SetFontSize(9).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["TotalSalidas"].ToString()).SetFontSize(9).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(item["StockActual"].ToString()).SetFontSize(9).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)));
                            table.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(Convert.ToDecimal(item["ValorInventario"]).ToString("C")).SetFontSize(9).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)));
                        }
                        
                        document.Add(table);
                    }
                }

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath),
                    Title = "Reporte de Rotación Almacén"
                });

                await DisplayAlert("✅ Éxito", "Reporte de almacén generado y abierto.", "Genial");
            }
            catch (Exception ex)
            {
                await DisplayAlert("❌ Error", $"No se pudo generar el PDF:\n{ex.Message}", "Entendido");
            }
        }
    }
}
`

### 4.5 Panel de Administrador (AdminPage)
Gestión total de usuarios.

#### 📄 Views/AdminPage.xaml
`xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:CRUD_LOGIN_MAUI.Views"
             x:Class="CRUD_LOGIN_MAUI.Views.AdminPage"
             Title="Administrador"
             Shell.NavBarIsVisible="False">

    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="15" BackgroundColor="#E3F2FD">

            <!-- Título -->
            <Label Text="Panel de Control - Administrador"
                   FontSize="22"
                   FontAttributes="Bold"
                   TextColor="#0D47A1"
                   HorizontalOptions="Center"/>

            <!-- Entradas de usuario -->
            <Entry x:Name="txtUsuario" Placeholder="👤 Usuario"/>
            <Entry x:Name="txtPassword" Placeholder="🔑 Contraseña" IsPassword="True"/>

            <!-- Picker de roles -->
            <Picker x:Name="pickerRol" Title="Seleccionar Rol">
                <Picker.ItemsSource>
                    <x:Array Type="{x:Type x:String}">
                        <x:String>Admin</x:String>
                        <x:String>Supervisor</x:String>
                        <x:String>Vendedor</x:String>
                    </x:Array>
                </Picker.ItemsSource>
            </Picker>

            <!-- Buscador -->
            <Entry x:Name="txtBuscar"
                   Placeholder="🔎 Buscar por ID, Usuario o Rol..."
                   TextChanged="OnSearchChanged"
                   BackgroundColor="White"/>

            <!-- Botones CRUD -->
            <Grid ColumnDefinitions="*,*,*" RowDefinitions="Auto,Auto" ColumnSpacing="10" RowSpacing="10">
                <Button Grid.Row="0" Grid.Column="0" Text="➕ Insertar" BackgroundColor="Green" TextColor="White" Clicked="OnInsertClicked"/>
                <Button Grid.Row="0" Grid.Column="1" Text="🔄 Actualizar" BackgroundColor="DodgerBlue" TextColor="White" Clicked="OnUpdateClicked"/>
                <Button Grid.Row="0" Grid.Column="2" Text="🗑️ Eliminar" BackgroundColor="Red" TextColor="White" Clicked="OnDeleteClicked"/>
                <Button Grid.Row="1" Grid.Column="0" Text="🔍 Consultar" BackgroundColor="Orange" TextColor="White" Clicked="OnConsultClicked"/>
                <Button Grid.Row="1" Grid.Column="1" Text="✅ Validar" BackgroundColor="Purple" TextColor="White" Clicked="OnValidateClicked"/>
                <Button Grid.Row="1" Grid.Column="2" Text="🧹 Limpiar" BackgroundColor="Gray" TextColor="White" Clicked="OnClearClicked"/>
            </Grid>

            <!-- Mensajes -->
            <Label x:Name="lblMensaje" TextColor="Red" FontAttributes="Bold" HorizontalOptions="Center"/>

            <!-- Lista de usuarios con compiled bindings -->
            <CollectionView x:Name="listaUsuarios"
                            SelectionMode="Single"
                            SelectionChanged="OnUsuarioSelected"
                            HeightRequest="250">
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="local:UsuarioItem">
                        <Grid Padding="10" ColumnDefinitions="40, *, *">
                            <Label Grid.Column="0" Text="{Binding Id}" FontAttributes="Bold" TextColor="Blue"/>
                            <Label Grid.Column="1" Text="{Binding Usuario}" FontAttributes="Bold"/>
                            <Label Grid.Column="2" Text="{Binding Rol}" TextColor="Gray"/>
                        </Grid>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>

            <Button Text="⚙️ Roles"
                    BackgroundColor="Teal"
                    TextColor="White"
                    Clicked="OnRolesClicked"/>


            <!-- Botón logout -->
            <Button Text="Cerrar sesión"
                    BackgroundColor="DarkRed"
                    TextColor="White"
                    Margin="0,20"
                    Clicked="OnLogoutClicked"/>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
`

#### 📄 Views/AdminPage.xaml.cs
`csharp
using CRUD_LOGIN_MAUI.Services;
using CRUD_LOGIN_MAUI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUD_LOGIN_MAUI.Views;

public class UsuarioItem
{
    public int Id { get; set; }
    public string Usuario { get; set; }
    public string Rol { get; set; }
}

public partial class AdminPage : ContentPage
{
    private string connectionString = ConfigDB.ConnectionString;
    private bool isProcessing = false;
    private int idSeleccionado = 0;
    private List<int> _rolesIds = new List<int>();

    public AdminPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarRoles();
    }

    private async Task CargarRoles()
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            string query = "SELECT Id, NombreRol FROM Roles";
            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            _rolesIds.Clear();
            var roles = new List<string>();
            while (await reader.ReadAsync())
            {
                _rolesIds.Add((int)reader["Id"]);
                roles.Add(reader["NombreRol"].ToString());
            }

            pickerRol.ItemsSource = roles;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los roles: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("🔒 Confirmación", "¿Estás seguro de que deseas cerrar tu sesión?", "Sí, salir", "No, quedarme");
        if (answer)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }

    private async void OnInsertClicked(object sender, EventArgs e) =>
        await EjecutarAccion(@"INSERT INTO Usuarios (Usuario, Password, IdRol) 
                               VALUES (@Usuario, CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2), @IdRol)", "insertar");

    private async void OnUpdateClicked(object sender, EventArgs e) =>
        await EjecutarAccion(@"UPDATE Usuarios 
                               SET Usuario=@Usuario, Password=CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2), IdRol=@IdRol 
                               WHERE Id=@Id", "actualizar");

    private async void OnDeleteClicked(object sender, EventArgs e) =>
        await EjecutarAccion("DELETE FROM Usuarios WHERE Id=@Id", "eliminar");

    private async Task EjecutarAccion(string query, string accion)
    {
        if (isProcessing) return;

        bool confirmar = await DisplayAlert("Confirmación", $"¿Seguro que desea {accion} este usuario?", "Sí", "No");
        if (!confirmar) return;

        if (pickerRol.SelectedIndex < 0)
        {
            await DisplayAlert("Error", "Debe seleccionar un rol.", "OK");
            return;
        }

        isProcessing = true;
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Usuario", txtUsuario.Text ?? "");
            cmd.Parameters.AddWithValue("@Password", txtPassword.Text ?? "");

            cmd.Parameters.AddWithValue("@IdRol", _rolesIds[pickerRol.SelectedIndex]);
            cmd.Parameters.AddWithValue("@Id", idSeleccionado);

            await cmd.ExecuteNonQueryAsync();
            await DisplayAlert("Éxito", $"Usuario {accion}do correctamente.", "OK");

            LimpiarCampos();
            await CargarLista("SELECT U.Id, U.Usuario, R.NombreRol FROM Usuarios U INNER JOIN Roles R ON U.IdRol = R.Id", "");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        finally { isProcessing = false; }
    }

    private async void OnConsultClicked(object sender, EventArgs e) =>
        await CargarLista(@"SELECT U.Id, U.Usuario, R.NombreRol 
                            FROM Usuarios U INNER JOIN Roles R ON U.IdRol = R.Id", "");

    private async void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        string filtro = "%" + e.NewTextValue + "%";
        string query = @"SELECT U.Id, U.Usuario, R.NombreRol 
                         FROM Usuarios U INNER JOIN Roles R ON U.IdRol = R.Id 
                         WHERE U.Usuario LIKE @Filtro OR CAST(U.Id AS VARCHAR) LIKE @Filtro";
        await CargarLista(query, filtro);
    }

    private async Task CargarLista(string query, string parametro)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            if (!string.IsNullOrEmpty(parametro)) cmd.Parameters.AddWithValue("@Filtro", parametro);

            using var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<UsuarioItem>();

            while (await reader.ReadAsync())
                lista.Add(new UsuarioItem
                {
                    Id = (int)reader["Id"],
                    Usuario = reader["Usuario"].ToString(),
                    Rol = reader["NombreRol"].ToString()
                });

            listaUsuarios.ItemsSource = lista;
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    private void OnUsuarioSelected(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as UsuarioItem;
        if (item != null)
        {
            idSeleccionado = item.Id;
            txtUsuario.Text = item.Usuario;
            txtPassword.Text = "";

            if (pickerRol.ItemsSource is List<string> roles)
            {
                pickerRol.SelectedIndex = roles.IndexOf(item.Rol);
            }
        }
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        string query = @"SELECT U.Usuario 
                         FROM Usuarios U 
                         WHERE U.Usuario=@U AND U.Password=CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @P), 2)";

        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@U", txtUsuario.Text);
        cmd.Parameters.AddWithValue("@P", txtPassword.Text);

        lblMensaje.Text = (await cmd.ExecuteScalarAsync() != null) ? "✅ Credenciales correctas" : "❌ Incorrecto";
    }

    private void OnClearClicked(object sender, EventArgs e) => LimpiarCampos();

    private async void OnRolesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RolesPage());
    }

    private void LimpiarCampos()
    {
        idSeleccionado = 0;
        txtUsuario.Text = txtPassword.Text = txtBuscar.Text = "";
        pickerRol.SelectedIndex = -1;
        lblMensaje.Text = "";
        listaUsuarios.ItemsSource = null;
    }
}
`

### 4.6 Gestión de Roles (RolesPage)
#### 📄 Views/RolesPage.xaml
`xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:CRUD_LOGIN_MAUI.Views"
             x:Class="CRUD_LOGIN_MAUI.Views.RolesPage"
             Title="Gestión de Roles">

    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="15" BackgroundColor="#FFFDE7">

            <Label Text="Panel de Control - Roles"
                   FontSize="22"
                   FontAttributes="Bold"
                   TextColor="#BF360C"
                   HorizontalOptions="Center"/>

            <!-- Campo para nombre del rol -->
            <Entry x:Name="txtRol" Placeholder="🛡️ Nombre del Rol"/>

            <!-- Buscador -->
            <Entry x:Name="txtBuscar"
                   Placeholder="🔎 Buscar por ID o Nombre..."
                   TextChanged="OnSearchChanged"
                   BackgroundColor="White"/>

            <!-- Botones CRUD -->
            <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto,Auto" ColumnSpacing="10" RowSpacing="10">
                <Button Grid.Row="0" Grid.Column="0" Text="➕ Insertar" BackgroundColor="Green" TextColor="White" Clicked="OnInsertClicked"/>
                <Button Grid.Row="0" Grid.Column="1" Text="🔄 Actualizar" BackgroundColor="DodgerBlue" TextColor="White" Clicked="OnUpdateClicked"/>
                <Button Grid.Row="1" Grid.Column="0" Text="🗑️ Eliminar" BackgroundColor="Red" TextColor="White" Clicked="OnDeleteClicked"/>
                <Button Grid.Row="1" Grid.Column="1" Text="🔍 Consultar" BackgroundColor="Orange" TextColor="White" Clicked="OnConsultClicked"/>
                <Button Grid.Row="2" Grid.Column="0" Grid.ColumnSpan="2" Text="🧹 Limpiar" BackgroundColor="Gray" TextColor="White" Clicked="OnClearClicked"/>
            </Grid>

            <!-- Lista de roles -->
            <CollectionView x:Name="listaRoles"
                            SelectionMode="Single"
                            SelectionChanged="OnRolSelected"
                            HeightRequest="250">
                <CollectionView.ItemTemplate>
                    <DataTemplate x:DataType="local:RolItem">
                        <Grid Padding="10" ColumnDefinitions="40, *">
                            <Label Grid.Column="0" Text="{Binding Id}" FontAttributes="Bold" TextColor="Blue"/>
                            <Label Grid.Column="1" Text="{Binding NombreRol}" FontAttributes="Bold"/>
                        </Grid>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>

            <!-- Botón logout -->
            <Button Text="Cerrar sesión"
                    BackgroundColor="DarkRed"
                    TextColor="White"
                    Margin="0,20"
                    Clicked="OnLogoutClicked"/>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
`
#### 📄 Views/RolesPage.xaml.cs
`csharp
using CRUD_LOGIN_MAUI.Services;
using CRUD_LOGIN_MAUI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUD_LOGIN_MAUI.Views;

public class RolItem
{
    public int Id { get; set; }
    public string NombreRol { get; set; }
}

public partial class RolesPage : ContentPage
{
    private string connectionString = ConfigDB.ConnectionString;
    private bool isProcessing = false;
    private int idSeleccionado = 0;

    public RolesPage() => InitializeComponent();

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("🔒 Confirmación", "¿Estás seguro de que deseas cerrar tu sesión?", "Sí, salir", "No, quedarme");
        if (answer)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }

    private async void OnInsertClicked(object sender, EventArgs e) =>
        await EjecutarAccion("INSERT INTO Roles (NombreRol) VALUES (@NombreRol)", "insertar");

    private async void OnUpdateClicked(object sender, EventArgs e) =>
        await EjecutarAccion("UPDATE Roles SET NombreRol=@NombreRol WHERE Id=@Id", "actualizar");

    private async void OnDeleteClicked(object sender, EventArgs e) =>
        await EjecutarAccion("DELETE FROM Roles WHERE Id=@Id", "eliminar");

    private async Task EjecutarAccion(string query, string accion)
    {
        if (isProcessing) return;

        bool confirmar = await DisplayAlert("Confirmación", $"¿Seguro que desea {accion} este rol?", "Sí", "No");
        if (!confirmar) return;

        isProcessing = true;
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@NombreRol", txtRol.Text ?? "");
            cmd.Parameters.AddWithValue("@Id", idSeleccionado);

            await cmd.ExecuteNonQueryAsync();
            await DisplayAlert("Éxito", $"Rol {accion}do correctamente.", "OK");

            LimpiarCampos();
            await CargarLista("SELECT Id, NombreRol FROM Roles", "");
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        finally { isProcessing = false; }
    }

    private async void OnConsultClicked(object sender, EventArgs e) =>
        await CargarLista("SELECT Id, NombreRol FROM Roles", "");

    private async void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        string filtro = "%" + e.NewTextValue + "%";
        string query = "SELECT Id, NombreRol FROM Roles WHERE NombreRol LIKE @Filtro OR CAST(Id AS VARCHAR) LIKE @Filtro";
        await CargarLista(query, filtro);
    }

    private async Task CargarLista(string query, string parametro)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            if (!string.IsNullOrEmpty(parametro)) cmd.Parameters.AddWithValue("@Filtro", parametro);

            using var reader = await cmd.ExecuteReaderAsync();
            var lista = new List<RolItem>();

            while (await reader.ReadAsync())
                lista.Add(new RolItem
                {
                    Id = (int)reader["Id"],
                    NombreRol = reader["NombreRol"].ToString()
                });

            listaRoles.ItemsSource = lista;
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }

    private void OnRolSelected(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as RolItem;
        if (item != null)
        {
            idSeleccionado = item.Id;
            txtRol.Text = item.NombreRol;
        }
    }

    private void OnClearClicked(object sender, EventArgs e) => LimpiarCampos();

    private void LimpiarCampos()
    {
        idSeleccionado = 0;
        txtRol.Text = txtBuscar.Text = "";
        listaRoles.ItemsSource = null;
    }
}
`

### 4.7 Vistas de Solo Lectura (SupervisorPage y VendedorPage)

#### 📄 Views/SupervisorPage.xaml
`xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="CRUD_LOGIN_MAUI.Views.SupervisorPage"
             Title="SupervisorPage"
             Shell.NavBarIsVisible="False">
    <VerticalStackLayout Padding="30" Spacing="20" BackgroundColor="#FFF3E0">
        <Label Text="Bienvenido Supervisor"
               FontSize="24"
               FontAttributes="Bold"
               TextColor="#E65100"
               HorizontalOptions="Center" 
               Margin="10"/>

        <!-- Logo -->
        <Image Source="https://cdn-icons-png.flaticon.com/256/3461/3461567.png"
                 HeightRequest="175"
                 HorizontalOptions="Center" />

        <Label Text="Ventana: Reportes y supervision"
               FontSize="18"
               TextColor="Black"
               HorizontalOptions="Center" 
                Margin="10" />

        <Button Text="📦 Gestionar Inventario" 
                Clicked="OnInventarioClicked" 
                BackgroundColor="#1976D2" 
                TextColor="White" 
                HorizontalOptions="Center" 
                WidthRequest="280"
                HeightRequest="50"
                Margin="0,10,0,0" />

        <Button Text="📊 Ver Reportes y PDF" 
                Clicked="OnReportesClicked" 
                BackgroundColor="#388E3C" 
                TextColor="White" 
                HorizontalOptions="Center" 
                WidthRequest="280"
                HeightRequest="50"
                Margin="0,10,0,0" />
                
        <Button Text="📈 VER DASHBOARD HISTÓRICO" 
                Clicked="OnVerReporteClicked" 
                BackgroundColor="#8b5cf6" 
                TextColor="White" 
                HorizontalOptions="Center" 
                WidthRequest="280"
                HeightRequest="50"
                Margin="0,10,0,0" />

        <Button Text="Cerrar Sesion" 
                Clicked="OnLogoutClicked" 
                BackgroundColor="#D32F2F" 
                TextColor="White" 
                HorizontalOptions="Center" 
                Margin="0,20,0,0" />
    </VerticalStackLayout>
</ContentPage>
`
#### 📄 Views/SupervisorPage.xaml.cs
`csharp
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
`
#### 📄 Views/VendedorPage.xaml
`xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:CRUD_LOGIN_MAUI.Models"
             x:Class="CRUD_LOGIN_MAUI.Views.VendedorPage"
             Title="Punto de Venta (POS)"
             BackgroundColor="#f8fafc"
             Shell.NavBarIsVisible="False">

    <Grid RowDefinitions="Auto, *, Auto" Padding="20">
        
        <!-- Header: Cliente y Producto -->
        <VerticalStackLayout Grid.Row="0" Spacing="15" Margin="0,0,0,20">
            <Label Text="🛍️ NUEVA VENTA" FontSize="24" FontAttributes="Bold" TextColor="#1e293b"/>
            
            <Frame BackgroundColor="White" Padding="15" CornerRadius="10" HasShadow="True" BorderColor="#cbd5e1">
                <Grid ColumnDefinitions="*, *" RowDefinitions="Auto, Auto" ColumnSpacing="15" RowSpacing="15">
                    
                    <Picker x:Name="pickerCliente" Title="👤 Seleccione Cliente" ItemDisplayBinding="{Binding Nombre}" Grid.Row="0" Grid.Column="0"/>
                    
                    <Picker x:Name="pickerProducto" Title="📦 Seleccione Producto" ItemDisplayBinding="{Binding Nombre}" Grid.Row="0" Grid.Column="1"/>
                    
                    <Entry x:Name="txtCantidad" Placeholder="Cantidad" Keyboard="Numeric" Grid.Row="1" Grid.Column="0"/>
                    
                    <Button Text="➕ Agregar al Carrito" BackgroundColor="#10b981" TextColor="White" Clicked="OnAgregarClicked" Grid.Row="1" Grid.Column="1"/>
                </Grid>
            </Frame>
        </VerticalStackLayout>

        <!-- Body: Carrito de Compras -->
        <Frame Grid.Row="1" BackgroundColor="White" Padding="10" CornerRadius="10" HasShadow="True" BorderColor="#cbd5e1">
            <VerticalStackLayout>
                <Grid ColumnDefinitions="3*, 1*, 2*, 2*" Padding="5" BackgroundColor="#e2e8f0">
                    <Label Text="Producto" FontAttributes="Bold" Grid.Column="0"/>
                    <Label Text="Cant" FontAttributes="Bold" HorizontalTextAlignment="Center" Grid.Column="1"/>
                    <Label Text="Precio" FontAttributes="Bold" HorizontalTextAlignment="End" Grid.Column="2"/>
                    <Label Text="Total" FontAttributes="Bold" HorizontalTextAlignment="End" Grid.Column="3"/>
                </Grid>
                
                <CollectionView x:Name="listaCarrito" Margin="0,10,0,0">
                    <CollectionView.ItemTemplate>
                        <DataTemplate x:DataType="models:DetalleVenta">
                            <Grid ColumnDefinitions="3*, 1*, 2*, 2*" Padding="5,10">
                                <Label Text="{Binding ProductoNombre}" VerticalOptions="Center" Grid.Column="0"/>
                                <Label Text="{Binding Cantidad}" HorizontalTextAlignment="Center" VerticalOptions="Center" Grid.Column="1"/>
                                <Label Text="{Binding PrecioVentaAplicado, StringFormat='{0:C}'}" HorizontalTextAlignment="End" VerticalOptions="Center" Grid.Column="2"/>
                                <Label Text="{Binding Total, StringFormat='{0:C}'}" FontAttributes="Bold" HorizontalTextAlignment="End" VerticalOptions="Center" Grid.Column="3" TextColor="#0f172a"/>
                            </Grid>
                        </DataTemplate>
                    </CollectionView.ItemTemplate>
                </CollectionView>
            </VerticalStackLayout>
        </Frame>

        <!-- Footer: Total y Botón de Cobrar -->
        <VerticalStackLayout Grid.Row="2" Spacing="15" Margin="0,20,0,0">
            <Frame BackgroundColor="#1e293b" Padding="20" CornerRadius="10">
                <Grid ColumnDefinitions="*, Auto">
                    <Label Text="TOTAL A PAGAR:" TextColor="White" FontSize="20" FontAttributes="Bold" VerticalOptions="Center" Grid.Column="0"/>
                    <Label x:Name="lblTotal" Text="$0.00" TextColor="#10b981" FontSize="28" FontAttributes="Bold" Grid.Column="1"/>
                </Grid>
            </Frame>
            
            <Button Text="💵 COBRAR E IMPRIMIR" BackgroundColor="#3b82f6" TextColor="White" FontSize="18" FontAttributes="Bold" HeightRequest="60" CornerRadius="10" Clicked="OnCobrarClicked"/>
            <Button Text="📊 VER REPORTE HISTÓRICO" BackgroundColor="#8b5cf6" TextColor="White" FontSize="16" FontAttributes="Bold" HeightRequest="50" CornerRadius="10" Clicked="OnVerReporteClicked"/>
            <Button Text="Cerrar sesión" BackgroundColor="#ef4444" TextColor="White" Margin="0,10,0,0" Clicked="OnLogoutClicked"/>
        </VerticalStackLayout>
    </Grid>
</ContentPage>
`
#### 📄 Views/VendedorPage.xaml.cs
`csharp
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
`

---

## ✅ CONCLUSIÓN Y BUENAS PRÁCTICAS
- **MVVM y Capas:** Al separar ConfigDB y VentaService de las vistas, hemos logrado un código limpio y mantenible.
- **Microservicios (API):** La pesada labor de renderizar PDFs ocurre en ASP.NET Core, dejando la app MAUI ligera.
- **Integridad SQL:** Nunca permitimos eliminar un producto que tenga historial de ventas, protegiendo las finanzas.
- **UI Responsiva:** El uso de Grid y CollectionView (con el truco InputTransparent="True") garantiza una experiencia impecable en móviles.
