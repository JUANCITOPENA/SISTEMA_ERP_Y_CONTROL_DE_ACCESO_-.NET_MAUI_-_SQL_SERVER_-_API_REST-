/********************************************************************************************
    PROYECTO EDUCATIVO: SISTEMA DE LOGIN CON ROLES Y CONTRASEÑAS ENCRIPTADAS (SHA2_256)
    -----------------------------------------------------------------------------------
    OBJETIVO:
    Crear la base "LoginRolesDB_cif" que implementa autenticación básica con roles 
    (Admin, Supervisor, Vendedor) y contraseñas encriptadas (SHA2_256).
********************************************************************************************/

-- 1. CREAR BASE DE DATOS PRINCIPAL
CREATE DATABASE LoginRolesDB_cif_MINI_ERP;
GO

-- SELECCIONAR LA BASE DE DATOS PARA TRABAJAR
USE LoginRolesDB_cif_MINI_ERP;
GO

-- CREAR TABLA DE ROLES
-- Esta tabla define los tipos de roles disponibles en el sistema.
-- Cada rol tiene un identificador único (Id) y un nombre descriptivo (NombreRol).
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NombreRol VARCHAR(50) NOT NULL
);

-- CREAR TABLA DE USUARIOS
-- Esta tabla almacena los datos de los usuarios del sistema.
-- La columna Password guarda el hash SHA2_256 de la contraseña, no el texto original.
-- IdRol establece la relación con la tabla Roles mediante clave foránea.
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Usuario VARCHAR(50) NOT NULL,
    Password VARCHAR(64) NOT NULL, -- HASH SHA2_256
    IdRol INT FOREIGN KEY REFERENCES Roles(Id)
);

-- INSERTAR ROLES PREDEFINIDOS
-- Se agregan tres roles básicos para el sistema: Admin, Supervisor y Vendedor.
INSERT INTO Roles (NombreRol) VALUES ('Admin'), ('Supervisor'), ('Vendedor'), ('Almacenista');

select * from roles

-- INSERTAR USUARIOS CON CONTRASEÑAS ENCRIPTADAS
-- Se usa la función HASHBYTES con el algoritmo SHA2_256 para generar el hash.
-- CONVERT(VARCHAR(64), ..., 2) transforma el resultado binario en texto hexadecimal.
INSERT INTO Usuarios (Usuario, Password, IdRol) VALUES
('AdminUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), 1),
('SuperUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'super123'), 2), 2),
('SalesUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'sales123'), 2), 3),
('AlmacenUser', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'almacen123'), 2), 4);
GO

-- CONSULTAR TODOS LOS USUARIOS REGISTRADOS
-- Muestra los datos almacenados en la tabla Usuarios.
SELECT * FROM Usuarios;

-- CONSULTAR TODOS LOS ROLES DISPONIBLES
-- Permite verificar los roles creados en la tabla Roles.
SELECT * FROM Roles;

-- CONSULTAR USUARIOS JUNTO A SUS ROLES
-- Realiza un INNER JOIN entre Usuarios y Roles para mostrar el nombre del rol asignado.
SELECT u.Usuario, u.Password, r.NombreRol
FROM Usuarios u
INNER JOIN Roles r ON u.IdRol = r.Id;
GO

-- VALIDAR LOGIN DE UN USUARIO (EJEMPLO: ADMINUSER)
-- Se declaran variables para simular el ingreso de credenciales.
DECLARE @Usuario VARCHAR(50) = 'AdminUser';
DECLARE @Password VARCHAR(50) = 'admin123';

-- Se compara el usuario y el hash de la contraseña ingresada con los datos almacenados.
-- Si coinciden, se devuelve el nombre del usuario y su rol correspondiente.
SELECT u.Usuario, r.NombreRol
FROM Usuarios u
INNER JOIN Roles r ON u.IdRol = r.Id
WHERE u.Usuario = @Usuario
AND u.Password = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password), 2);
GO


-- Ejecuta esto y mira el resultado en la columna "HashCalculado"
SELECT 
    Usuario, 
    Password AS PasswordGuardado,
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2) AS HashCalculado
FROM Usuarios 
WHERE Usuario = 'AdminUser';



-- 1. CATEGORIA
CREATE TABLE Categoria (
    Id      INT          IDENTITY(1,1) PRIMARY KEY,
    Nombre  VARCHAR(100) NOT NULL
);

-- 2. PRODUCTO
CREATE TABLE Producto (
    Id           INT           IDENTITY(1,1) PRIMARY KEY,
    Nombre       VARCHAR(100)  NOT NULL,
    CategoriaId  INT           NOT NULL,
    PrecioCompra DECIMAL(10,2) NOT NULL,
    PrecioVenta  DECIMAL(10,2) NOT NULL,
    Stock        INT           NOT NULL,
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (CategoriaId) REFERENCES Categoria(Id)
);

-- 3. CLIENTE
CREATE TABLE Cliente (
    Id       INT          IDENTITY(1,1) PRIMARY KEY,
    Nombre   VARCHAR(100) NOT NULL,
    RNC      VARCHAR(20),
    Telefono VARCHAR(20)
);

-- 4. VENDEDOR
CREATE TABLE Vendedor (
    Id     INT          IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Codigo VARCHAR(20)  NOT NULL UNIQUE
);

-- 5. VENTAS (Cabecera de la factura)
CREATE TABLE Ventas (
    Id         INT      IDENTITY(1,1) PRIMARY KEY,
    Fecha      DATETIME NOT NULL DEFAULT GETDATE(),
    ClienteId  INT      NOT NULL,
    VendedorId INT      NOT NULL,
    CONSTRAINT FK_Ventas_Cliente  FOREIGN KEY (ClienteId)  REFERENCES Cliente(Id),
    CONSTRAINT FK_Ventas_Vendedor FOREIGN KEY (VendedorId) REFERENCES Vendedor(Id)
);

-- 6. DETALLE_VENTAS (Líneas de los productos con Columnas Calculadas)
CREATE TABLE Detalle_Ventas (
    Id                  INT           IDENTITY(1,1) PRIMARY KEY,
    VentaId             INT           NOT NULL,
    ProductoId          INT           NOT NULL,
    Cantidad            INT           NOT NULL,
    PrecioVentaAplicado DECIMAL(10,2) NOT NULL,

    -- Cálculos automáticos guardados físicamente
    SubTotal AS (Cantidad * PrecioVentaAplicado)               PERSISTED,
    Itbis    AS ((Cantidad * PrecioVentaAplicado) * 0.18)      PERSISTED,
    Total    AS ((Cantidad * PrecioVentaAplicado) * 1.18)      PERSISTED,

    CONSTRAINT FK_DetalleVentas_Ventas   FOREIGN KEY (VentaId)    REFERENCES Ventas(Id),
    CONSTRAINT FK_DetalleVentas_Producto FOREIGN KEY (ProductoId) REFERENCES Producto(Id)
);
GO


/* ============================================================
   1. CATEGORIA (10 registros)
   ============================================================ */
INSERT INTO Categoria (Nombre) VALUES
('Electrónica'), ('Hogar'), ('Computación'), ('Ropa'), ('Calzado'),
('Juguetes'), ('Bebidas'), ('Alimentos'), ('Accesorios'), ('Herramientas');

/* ============================================================
   2. PRODUCTO (10 registros)
   ============================================================ */
INSERT INTO Producto (Nombre, CategoriaId, PrecioCompra, PrecioVenta, Stock) VALUES
('Laptop Lenovo', 3, 45000, 52000, 10),
('Smartphone Samsung A54', 1, 18000, 23000, 25),
('Televisor LG 55"', 1, 38000, 45000, 8),
('Camiseta Nike', 4, 800, 1500, 50),
('Zapatos Adidas', 5, 2500, 4200, 30),
('Destornillador Pro', 10, 150, 350, 100),
('Whisky Jack Daniels', 7, 1200, 1800, 40),
('Arroz Premium 10lb', 8, 250, 450, 60),
('Mouse Logitech', 3, 600, 1200, 35),
('Audífonos Sony', 1, 900, 1600, 20);

/* ============================================================
   3. CLIENTE (10 registros)
   ============================================================ */
INSERT INTO Cliente (Nombre, RNC, Telefono) VALUES
('Juan Pérez', '001234567', '809-555-1001'),
('María López', '002345678', '809-555-1002'),
('Carlos Gómez', '003456789', '809-555-1003'),
('Ana Martínez', '004567890', '809-555-1004'),
('Pedro Sánchez', '005678901', '809-555-1005'),
('Laura Torres', '006789012', '809-555-1006'),
('José Ramírez', '007890123', '809-555-1007'),
('Luisa Fernández', '008901234', '809-555-1008'),
('Ricardo Díaz', '009012345', '809-555-1009'),
('Sofía Herrera', '010123456', '809-555-1010');

/* ============================================================
   4. VENDEDOR (10 registros)
   ============================================================ */
INSERT INTO Vendedor (Nombre, Codigo) VALUES
('Luis Peña', 'VEN001'), ('Carlos Ruiz', 'VEN002'),
('Marcos Castillo', 'VEN003'), ('Daniela Ortiz', 'VEN004'),
('Fernanda Cruz', 'VEN005'), ('Miguel Santos', 'VEN006'),
('Rosa Jiménez', 'VEN007'), ('Javier Molina', 'VEN008'),
('Patricia Núñez', 'VEN009'), ('Samuel Batista', 'VEN010');

/* ============================================================
   5. VENTAS (10 Facturas)
   ============================================================ */
INSERT INTO Ventas (ClienteId, VendedorId) VALUES
(1, 1), (2, 2), (3, 3), (4, 4), (5, 5), 
(6, 6), (7, 7), (8, 8), (9, 9), (10, 10);

/* ============================================================
   6. DETALLE_VENTAS (15 líneas de productos distribuidas)
   ============================================================ */
INSERT INTO Detalle_Ventas (VentaId, ProductoId, Cantidad, PrecioVentaAplicado) VALUES
(1, 1, 1, 52000), -- Factura 1
(1, 2, 1, 23000), -- Factura 1 (2do producto)
(2, 2, 2, 23000), -- Factura 2
(3, 3, 1, 45000), -- Factura 3
(3, 4, 2, 1500),  -- Factura 3 (2do producto)
(4, 4, 3, 1500),  -- Factura 4
(5, 5, 1, 4200),  -- Factura 5
(5, 7, 3, 1800),  -- Factura 5 (2do producto)
(6, 6, 5, 350),   -- Factura 6
(7, 7, 2, 1800),  -- Factura 7
(7, 1, 1, 52000), -- Factura 7 (2do producto)
(8, 8, 4, 450),   -- Factura 8
(9, 9, 2, 1200),  -- Factura 9
(9, 5, 2, 4200),  -- Factura 9 (2do producto)
(10, 10, 1, 1600);-- Factura 10
GO

/* ============================================================
   CONSULTAS Y JOINS PARA EL NUEVO MODELO
   ============================================================ */

-- JOIN COMPLETO PARA VER CABECERA Y DETALLE (IDs + Nombres)

SELECT 
    v.Id AS NumeroFactura,
    v.Fecha,
    c.Nombre AS Cliente,
    ven.Nombre AS Vendedor,
    p.Nombre AS Producto,
    dv.Cantidad,
    dv.PrecioVentaAplicado AS PrecioUnidad,
    dv.SubTotal,
    dv.Itbis,
    dv.Total AS TotalLinea
FROM Ventas v
INNER JOIN Detalle_Ventas dv ON v.Id = dv.VentaId
INNER JOIN Cliente c         ON v.ClienteId = c.Id
INNER JOIN Vendedor ven      ON v.VendedorId = ven.Id
INNER JOIN Producto p        ON dv.ProductoId = p.Id
ORDER BY v.Id ASC;

-- JOIN PARA VER EL TOTAL CONSOLIDADO POR FACTURA

SELECT 
    v.Id AS NumeroFactura,
    v.Fecha,
    c.Nombre AS Cliente,
    ven.Nombre AS Vendedor,
    SUM(dv.Cantidad) AS TotalArticulos,
    SUM(dv.SubTotal) AS SubTotalFactura,
    SUM(dv.Itbis) AS ItbisFactura,
    SUM(dv.Total) AS TotalPagarFactura
FROM Ventas v
INNER JOIN Detalle_Ventas dv ON v.Id = dv.VentaId
INNER JOIN Cliente c         ON v.ClienteId = c.Id
INNER JOIN Vendedor ven      ON v.VendedorId = ven.Id
GROUP BY v.Id, v.Fecha, c.Nombre, ven.Nombre
ORDER BY v.Id ASC;


--CREA VISTA MOVIMIENTO DE ALMACEN:

CREATE VIEW vw_ReporteInventario AS
SELECT 
    p.Id AS ProductoId,
    p.Nombre AS Producto,
    c.Nombre AS Categoria,
    p.PrecioCompra,
    p.PrecioVenta,
    p.Stock AS StockActual,
    ISNULL((SELECT SUM(Cantidad) FROM Detalle_Ventas WHERE ProductoId = p.Id), 0) AS TotalSalidas,
    p.Stock + ISNULL((SELECT SUM(Cantidad) FROM Detalle_Ventas WHERE ProductoId = p.Id), 0) AS StockInicialEntradas,
    (p.Stock * p.PrecioCompra) AS ValorInventario
FROM Producto p
LEFT JOIN Categoria c ON p.CategoriaId = c.Id;


-- ejeutar la vista"

select * from vw_ReporteInventario
