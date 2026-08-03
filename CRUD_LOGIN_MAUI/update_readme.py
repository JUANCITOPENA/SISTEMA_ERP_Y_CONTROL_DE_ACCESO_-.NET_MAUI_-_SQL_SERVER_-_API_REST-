import re
import sys

with open('README_MAESTRO_V2.md', 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update Table of Contents
toc_replacement = """| 🛍️ | [Paso 7](#️-paso-7-vistas-limitadas-vendedor) | Vista Restringida — Vendedor |
| 📦 | [Paso 8](#-paso-8-arquitectura-por-capas-ventaservice) | Arquitectura por Capas (`VentaService`) |
| 📋 | [Paso 9](#-paso-9-módulo-de-inventario-crud-avanzado) | Módulo de Inventario (CRUD Avanzado) |
| 📊 | [Paso 10](#-paso-10-dashboard-del-almacenista) | Dashboard del Almacenista |
| ✅ | [Cierre](#-conclusión) | Conclusión y buenas prácticas |"""
content = re.sub(r'\| 🛍️ \| \[Paso 7\]\(.*?\) \| Vista Restringida — Vendedor \|\n\| ✅ \| \[Cierre\]\(.*?\) \| Conclusión y buenas prácticas \|', toc_replacement, content)

# 2. Update Architecture Diagram
arch_replacement = """┌─────────────┬─────────────────┬─────────────────┬─────────────────┐
│  AdminPage  │ SupervisorPage  │  VendedorPage   │ AlmacenistaPage │
│ (CRUD total)│ (solo lectura)  │  (solo ventas)  │ (Inventario)    │
└─────────────┴─────────────────┴─────────────────┴─────────────────┘"""
content = re.sub(r'┌─────────────┬─────────────────┬─────────────────┐\n│  AdminPage  │ SupervisorPage  │  VendedorPage    │\n│ \(CRUD total\)│ \(solo lectura\)  │  \(solo lectura\)  │\n└─────────────┴─────────────────┴─────────────────┘', arch_replacement, content)

# 3. Update Pre-requisites
req_replacement = """| 📦 Paquete NuGet `Microsoft.Data.SqlClient` | Conectar la app MAUI con SQL Server |
| 📄 Paquete NuGet `itext7.bouncy-castle-adapter` | Generación de reportes PDF avanzados |"""
content = re.sub(r'\| 📦 Paquete NuGet `Microsoft\.Data\.SqlClient` \| Conectar la app MAUI con SQL Server \|', req_replacement, content)

# 4. Update SQL Script
sql_original = """-- INSERTAR USUARIOS CON CONTRASEÑAS ENCRIPTADAS
-- Se usa la función HASHBYTES con el algoritmo SHA2_256 para generar el hash.
-- CONVERT(VARCHAR(64), ..., 2) transforma el resultado binario en texto hexadecimal.
INSERT INTO Usuarios (Usuario, Password, IdRol) VALUES
('AdminUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), 1),
('SuperUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'super123'), 2), 2),
('SalesUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'sales123'), 2), 3);
GO"""

sql_new = """-- INSERTAR ROLES ADICIONALES
INSERT INTO Roles (NombreRol) VALUES ('Almacenista');

-- INSERTAR USUARIOS CON CONTRASEÑAS ENCRIPTADAS
-- Se usa la función HASHBYTES con el algoritmo SHA2_256 para generar el hash.
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
    ClienteId INT NULL, -- Simplificado
    VendedorId INT NULL -- Simplificado
);

CREATE TABLE Detalle_Ventas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    VentaId INT FOREIGN KEY REFERENCES Ventas(Id),
    ProductoId INT FOREIGN KEY REFERENCES Producto(Id),
    Cantidad INT NOT NULL,
    PrecioVentaAplicado DECIMAL(10,2) NOT NULL -- Histórico Inmutable
);
GO

-- VISTA DE INVENTARIO PARA REPORTES
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
GO"""

content = content.replace(sql_original, sql_new)

# 5. Update AppShell.xaml.cs Routing
route_original = """        Routing.RegisterRoute("SupervisorPage", typeof(SupervisorPage));
        Routing.RegisterRoute("VendedorPage", typeof(VendedorPage));
        Routing.RegisterRoute("RolesPage", typeof(RolesPage));"""

route_new = """        Routing.RegisterRoute("SupervisorPage", typeof(SupervisorPage));
        Routing.RegisterRoute("VendedorPage", typeof(VendedorPage));
        Routing.RegisterRoute("RolesPage", typeof(RolesPage));
        Routing.RegisterRoute("AlmacenistaPage", typeof(AlmacenistaPage));
        Routing.RegisterRoute("InventarioPage", typeof(InventarioPage));"""

content = content.replace(route_original, route_new)

# 6. Append New Steps before "Buenas Prácticas"
new_sections = """---

## 📦 PASO 8: Arquitectura por Capas (`VentaService`)

Como buena práctica, extraeremos la lógica de la base de datos fuera de las vistas XAML. Crearemos una clase de servicio que maneje el CRUD y la generación de reportes.

1. Crea una carpeta `Services` en la raíz del proyecto.
2. Agrega la clase `VentaService.cs`.

### 📄 `VentaService.cs` (Lógica Centralizada)

```csharp
using Microsoft.Data.SqlClient;

namespace CRUD_LOGIN_MAUI.Services;

public class VentaService
{
    private readonly string _connectionString = "Server=10.0.0.15,1433;Database=LoginRolesDB_cif;User Id=JUANCITO;Password=123456;TrustServerCertificate=True;";

    // Validar dependencias antes de eliminar para mantener Integridad Referencial
    public async Task<(bool Exito, string Mensaje)> DeleteProductoAsync(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // 1. Verificar si existen ventas históricas de este producto
        using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Detalle_Ventas WHERE ProductoId = @Id", conn);
        checkCmd.Parameters.AddWithValue("@Id", id);
        int count = (int)await checkCmd.ExecuteScalarAsync();
        
        if (count > 0)
        {
            return (false, "No se puede eliminar este producto porque ya tiene ventas (salidas) asociadas en el sistema.");
        }

        // 2. Si no hay dependencias, se elimina
        using var deleteCmd = new SqlCommand("DELETE FROM Producto WHERE Id = @Id", conn);
        deleteCmd.Parameters.AddWithValue("@Id", id);
        await deleteCmd.ExecuteNonQueryAsync();
        
        return (true, "Producto eliminado correctamente.");
    }
}
```

> 🧠 **Concepto de Diseño (Precio Histórico Inmutable):** En los sistemas ERP reales, cuando se hace una venta, el precio actual del producto se guarda en `Detalle_Ventas.PrecioVentaAplicado`. Si el día de mañana el administrador sube el precio del producto en el inventario, **las facturas y ventas del pasado no se verán alteradas**.

---

## 📋 PASO 9: Módulo de Inventario (CRUD Avanzado)

Crearemos `InventarioPage.xaml` para que el **Almacenista** pueda gestionar los productos. Esta pantalla incluye un buscador en tiempo real y la capacidad de autollenar los campos al seleccionar un ítem.

### 🎨 `InventarioPage.xaml` (UI con Buscador y Grid Responsivo)

```xml
<Grid RowDefinitions="Auto, *">
    <!-- Formulario de Campos -->
    <VerticalStackLayout>
        <!-- ... Entradas (Nombre, Costo, Venta, Stock) ... -->
        
        <!-- Grid 2x2 para botones en Móvil (Evita desbordamiento) -->
        <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto" ColumnSpacing="15" RowSpacing="10" Margin="0,10">
            <Button Grid.Row="0" Grid.Column="0" Text="GUARDAR" Clicked="OnGuardar" BackgroundColor="Green" />
            <Button Grid.Row="0" Grid.Column="1" Text="LIMPIAR" Clicked="OnLimpiar" BackgroundColor="Gray" />
            <Button Grid.Row="1" Grid.Column="0" Text="ELIMINAR" Clicked="OnEliminar" BackgroundColor="Red" />
            <Button Grid.Row="1" Grid.Column="1" Text="GENERAR PDF" Clicked="OnGenerarPDF" BackgroundColor="DarkBlue" />
        </Grid>
    </VerticalStackLayout>

    <!-- Buscador Dinámico -->
    <SearchBar x:Name="searchBar" Placeholder="Buscar por nombre o categoría..." TextChanged="OnSearchTextChanged" />

    <!-- Lista de Productos -->
    <CollectionView x:Name="ListaProductos" SelectionMode="Single" SelectionChanged="OnSelectionChanged">
        <CollectionView.ItemTemplate>
            <DataTemplate>
                <!-- TRUCO MAUI: InputTransparent="True" permite que el clic pase a la CollectionView -->
                <Frame InputTransparent="True"> 
                    <Label Text="{Binding Nombre}" />
                </Frame>
            </DataTemplate>
        </CollectionView.ItemTemplate>
    </CollectionView>
</Grid>
```

> ⚠️ **El truco de MAUI:** A menudo, envolver elementos en un `Frame` o `Border` dentro de una lista bloquea el evento `SelectionChanged`. La solución mágica es aplicar `InputTransparent="True"` a la tarjeta (Frame), lo que hace que los "toques" de pantalla perforen visualmente el recuadro y sean captados por la fila de la lista.

### ⚙️ Lógica de Buscador y Selección

```csharp
// Filtrado en memoria usando LINQ
private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
{
    var keyword = e.NewTextValue?.ToLowerInvariant();
    var allProducts = await service.GetProductosAsync(); // Asume obtención asíncrona

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
```

---

## 📊 PASO 10: Dashboard del Almacenista

Finalmente, el `AlmacenistaPage.xaml` es la vista inicial (Dashboard) donde entra nuestro usuario `AlmUser`. En vez de listas simples, utilizamos un diseño moderno con `Border` para generar "Tarjetas (Cards)" con KPIs.

### 🎨 Visualización de KPIs (Stock y Movimientos)

```xml
<Border BackgroundColor="White" Padding="15" StrokeShape="RoundRectangle 10">
    <Grid ColumnDefinitions="*,*,*">
        <VerticalStackLayout Grid.Column="0">
            <Label Text="Entradas Iniciales" FontSize="11" />
            <Label Text="{Binding StockInicialEntradas}" FontAttributes="Bold" />
        </VerticalStackLayout>
        <VerticalStackLayout Grid.Column="1">
            <Label Text="Total Salidas" FontSize="11" />
            <Label Text="{Binding TotalSalidas}" TextColor="Red" />
        </VerticalStackLayout>
        <VerticalStackLayout Grid.Column="2">
            <Label Text="Stock Actual" FontSize="11" />
            <Label Text="{Binding StockActual}" TextColor="Green" />
        </VerticalStackLayout>
    </Grid>
</Border>
```

> 💡 **Matemática del Inventario:** El sistema no guarda el "Stock Inicial" estáticamente para siempre. En su lugar, usa la vista SQL `vw_ReporteInventario` para calcular dinámicamente: `Stock Inicial = Stock Actual + Total de Salidas`. Así siempre existe una trazabilidad perfecta sin desincronizaciones en la BD.

"""

content = content.replace("## 🧱 Buenas Prácticas y Consideraciones Adicionales", new_sections + "\n## 🧱 Buenas Prácticas y Consideraciones Adicionales")

with open('README_MAESTRO_V2.md', 'w', encoding='utf-8') as f:
    f.write(content)
print("README Actualizado con Exito.")
