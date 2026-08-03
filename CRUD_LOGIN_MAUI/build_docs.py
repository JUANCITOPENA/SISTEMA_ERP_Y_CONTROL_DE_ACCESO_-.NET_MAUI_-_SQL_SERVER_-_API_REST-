import os

project_root = r"C:\Users\User\Desktop\CRUD-2026\CRUD_LOGIN_MAUI"
api_root = os.path.join(project_root, "CRUD_LOGIN_MAUI.Api")
maui_root = os.path.join(project_root, "CRUD_LOGIN_MAUI")

def get_file_content(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return f.read().strip()
    except Exception as e:
        return f"// Error reading {path}: {e}"

markdown = f"""# 📘 BIBLIA DE INGENIERÍA JPV PRO V2.0: SISTEMA ERP Y CONTROL DE ACCESO (.NET MAUI + SQL SERVER + API REST)

Bienvenido al manual maestro y definitivo. Este documento contiene **ABSOLUTAMENTE TODO** el código, la arquitectura, la estructura del proyecto y las explicaciones pedagógicas de cada módulo para que el estudiante pueda replicarlo, entenderlo y llevarlo a producción.

---

## 📂 ESTRUCTURA DEL PROYECTO

Nuestra solución utiliza una arquitectura limpia separada en capas (MVC/MVVM guiado), compuesta por los siguientes proyectos:

1. **CRUD_LOGIN_MAUI**: La aplicación cliente multiplataforma.
2. **CRUD_LOGIN_MAUI.Api**: El backend (API REST) encargado de servicios pesados como la generación de PDFs.
3. **CRUD_LOGIN_MAUI.Tests**: Pruebas unitarias e integrales para garantizar la estabilidad.

```text
CRUD_LOGIN_MAUI/
│
├── CRUD_LOGIN_MAUI.Api/               <-- Backend (.NET Core Web API)
│   ├── Program.cs
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
│   │   ├── Producto.cs
│   │   ├── Usuario.cs
│   │   └── ...
│   ├── Services/                      <-- Lógica de Negocio y Conexiones
│   │   ├── ConfigDB.cs                <-- ¡Centralización de la Cadena de Conexión!
│   │   ├── VentaService.cs
│   │   └── TicketPdfService.cs
│   └── Views/                         <-- Interfaces de Usuario (XAML + CS)
│       ├── MainPage.xaml              <-- Login
│       ├── AdminPage.xaml             <-- Panel de Administrador
│       ├── AlmacenistaPage.xaml       <-- Dashboard de Almacén
│       ├── InventarioPage.xaml        <-- CRUD de Productos
│       └── ...
│
└── CRUD_LOGIN_MAUI.Tests/             <-- Pruebas (xUnit)
```

---

## 🗄️ PASO 1: BASE DE DATOS Y CONEXIÓN

Antes de tocar el código C#, necesitamos la estructura de datos. Aquí crearemos las tablas relacionales para Usuarios, Roles, Productos, Categorías y Ventas.

### 📜 Script SQL Completo (Ejecutar en SQL Server)

```sql
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
```

---

## ⚙️ PASO 2: CAPA DE SERVICIOS (Conexión Centralizada)

El error más común de los principiantes es poner la cadena de conexión (`ConnectionString`) regada por todas las ventanas. Nosotros utilizamos un patrón centralizado en `Services/ConfigDB.cs`.

### 📄 `Services/ConfigDB.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'Services', 'ConfigDB.cs'))}
```

### 📄 `Services/VentaService.cs`
La lógica de base de datos para ventas e inventarios se encuentra aquí, aislando las consultas de la interfaz gráfica.
```csharp
{get_file_content(os.path.join(maui_root, 'Services', 'VentaService.cs'))}
```

### 📄 `Services/TicketPdfService.cs`
Este servicio se comunica con nuestra API REST para generar PDFs, en vez de sobrecargar la app móvil.
```csharp
{get_file_content(os.path.join(maui_root, 'Services', 'TicketPdfService.cs'))}
```

---

## 🚀 PASO 3: EL BACKEND (API REST PARA PDF)

Nuestra aplicación delega el trabajo pesado (como crear PDFs) a una API en ASP.NET Core.

### 📄 `CRUD_LOGIN_MAUI.Api / Program.cs`
```csharp
{get_file_content(os.path.join(api_root, 'Program.cs'))}
```

### 📄 `CRUD_LOGIN_MAUI.Api / Controllers / PdfController.cs`
```csharp
{get_file_content(os.path.join(api_root, 'Controllers', 'PdfController.cs'))}
```

### 📄 `CRUD_LOGIN_MAUI.Api / Services / TicketPdfGenerator.cs`
Usamos iText7 para dibujar el PDF exacto (factura térmica 80mm).
```csharp
{get_file_content(os.path.join(api_root, 'Services', 'TicketPdfGenerator.cs'))}
```

---

## 🎨 PASO 4: INTERFACES GRÁFICAS (VISTAS MAUI)

A continuación, el código completo de todas nuestras pantallas (XAML y C#). 

### 4.1 Configuración de Rutas (`AppShell`)
Para navegar entre pantallas, registramos las rutas en el Shell.

#### 📄 `AppShell.xaml`
```xml
{get_file_content(os.path.join(maui_root, 'AppShell.xaml'))}
```

#### 📄 `AppShell.xaml.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'AppShell.xaml.cs'))}
```

### 4.2 Pantalla de Login (`MainPage`)
Controla el acceso y redirige según el Rol del usuario utilizando consultas seguras a `ConfigDB`.

#### 📄 `Views/MainPage.xaml`
```xml
{get_file_content(os.path.join(maui_root, 'Views', 'MainPage.xaml'))}
```

#### 📄 `Views/MainPage.xaml.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'Views', 'MainPage.xaml.cs'))}
```

### 4.3 Panel de Inventario (`InventarioPage`)
CRUD completo de productos con buscador en tiempo real y protección de integridad referencial.

#### 📄 `Views/InventarioPage.xaml`
```xml
{get_file_content(os.path.join(maui_root, 'Views', 'InventarioPage.xaml'))}
```

#### 📄 `Views/InventarioPage.xaml.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'Views', 'InventarioPage.xaml.cs'))}
```

### 4.4 Dashboard del Almacenista (`AlmacenistaPage`)
Tarjetas KPIs mostrando Stock Inicial, Salidas y Stock Actual dinámicamente.

#### 📄 `Views/AlmacenistaPage.xaml`
```xml
{get_file_content(os.path.join(maui_root, 'Views', 'AlmacenistaPage.xaml'))}
```

#### 📄 `Views/AlmacenistaPage.xaml.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'Views', 'AlmacenistaPage.xaml.cs'))}
```

### 4.5 Panel de Administrador (`AdminPage`)
Gestión total de usuarios.

#### 📄 `Views/AdminPage.xaml`
```xml
{get_file_content(os.path.join(maui_root, 'Views', 'AdminPage.xaml'))}
```

#### 📄 `Views/AdminPage.xaml.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'Views', 'AdminPage.xaml.cs'))}
```

### 4.6 Gestión de Roles (`RolesPage`)
#### 📄 `Views/RolesPage.xaml`
```xml
{get_file_content(os.path.join(maui_root, 'Views', 'RolesPage.xaml'))}
```
#### 📄 `Views/RolesPage.xaml.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'Views', 'RolesPage.xaml.cs'))}
```

### 4.7 Vistas de Solo Lectura (`SupervisorPage` y `VendedorPage`)

#### 📄 `Views/SupervisorPage.xaml`
```xml
{get_file_content(os.path.join(maui_root, 'Views', 'SupervisorPage.xaml'))}
```
#### 📄 `Views/SupervisorPage.xaml.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'Views', 'SupervisorPage.xaml.cs'))}
```
#### 📄 `Views/VendedorPage.xaml`
```xml
{get_file_content(os.path.join(maui_root, 'Views', 'VendedorPage.xaml'))}
```
#### 📄 `Views/VendedorPage.xaml.cs`
```csharp
{get_file_content(os.path.join(maui_root, 'Views', 'VendedorPage.xaml.cs'))}
```

---

## ✅ CONCLUSIÓN Y BUENAS PRÁCTICAS
- **MVVM y Capas:** Al separar `ConfigDB` y `VentaService` de las vistas, hemos logrado un código limpio y mantenible.
- **Microservicios (API):** La pesada labor de renderizar PDFs ocurre en ASP.NET Core, dejando la app MAUI ligera.
- **Integridad SQL:** Nunca permitimos eliminar un producto que tenga historial de ventas, protegiendo las finanzas.
- **UI Responsiva:** El uso de `Grid` y `CollectionView` (con el truco `InputTransparent="True"`) garantiza una experiencia impecable en móviles.

"""

with open(os.path.join(project_root, 'README_MAESTRO_V3.md'), 'w', encoding='utf-8') as f:
    f.write(markdown)

print("¡README_MAESTRO_V3.md generado con éxito!")
