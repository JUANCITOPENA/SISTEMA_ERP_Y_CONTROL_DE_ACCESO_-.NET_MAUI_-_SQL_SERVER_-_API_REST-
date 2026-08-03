# 🏆 MANUAL MAESTRO SUPREMO
## Sistema Empresarial JPV Pro V2.0
### Guía Definitiva de Construcción, Replicación y Comprensión 100%
#### `.NET MAUI` · `SQL Server` · `QuestPDF` · `Arquitectura Relacional`

> 📘 **¿Para quién es este manual?**
> Este documento está diseñado para desarrolladores que desean construir una aplicación empresarial completa desde cero, entendiendo **cada línea de código**, cada decisión de arquitectura y cada componente del sistema. No es solo un "copia y pega": es un tutorial narrativo que te explica el **por qué** detrás del **cómo**.

---

## 📑 Tabla de Contenidos

1. [Introducción y Visión del Proyecto](#1-introducción-y-visión-del-proyecto)
2. [El Stack Técnico Elite](#2-el-stack-técnico-elite)
3. [Planteamiento del Problema y Solución JPV](#3-planteamiento-del-problema-y-solución-jpv)
4. [Estructura Completa del Proyecto](#4-estructura-completa-del-proyecto)
5. [El Cimiento: Base de Datos SQL Server](#5-el-cimiento-base-de-datos-sql-server)
6. [Código Fuente Comentado Línea a Línea](#6-código-fuente-comentado-línea-a-línea)
   - [6.1 Los Modelos (Models/Venta.cs)](#61-los-modelos-modelsventa.cs)
   - [6.2 La Configuración de Conexión (Services/ConfigDB.cs)](#62-la-configuración-de-conexión-servicesconfigdbcs)
   - [6.3 La Navegación (AppShell.xaml)](#63-la-navegación-appshellxaml)
   - [6.4 Servicio de Datos Maestro (Services/VentaService.cs)](#64-servicio-de-datos-maestro-servicesventa-servicecs)
7. [Generación del Ticket PDF con QuestPDF](#7-generación-del-ticket-pdf-con-questpdf)
8. [Registro de Servicios (MauiProgram.cs)](#8-registro-de-servicios-mauiprogramcs)
9. [Conclusión y Próximos Pasos](#9-conclusión-y-próximos-pasos)

---

## 1. 🌟 Introducción y Visión del Proyecto

### ¿Qué es JPV Pro V2.0?

**JPV Pro V2.0** es un sistema de gestión empresarial multiplataforma construido con **.NET MAUI**. Representa la evolución de un CRUD básico hacia una arquitectura profesional completa, capaz de correr en **Windows, Android, iOS y macOS** desde una única base de código.

### ¿Qué hace diferente a este sistema?

A diferencia de aplicaciones simples con una sola tabla, JPV Pro V2.0 implementa:

| Característica | Descripción |
|---|---|
| 🗄️ **Base de datos relacional** | 5 tablas interconectadas con integridad referencial |
| ⚡ **Cálculos automáticos en BD** | SQL Server calcula ITBIS y Total con columnas persistidas |
| 📄 **Facturación física** | Generación de tickets PDF de 80mm para impresoras térmicas |
| 🔍 **Reportes dinámicos** | Filtros cruzados por cliente, vendedor y producto |
| 🔗 **Sincronización infalible** | Seleccionar un registro en lista activa automáticamente el formulario de edición |

### 🎯 Objetivo de Aprendizaje

Al terminar este manual serás capaz de:
- Diseñar y crear una base de datos relacional con T-SQL
- Conectar una app .NET MAUI a SQL Server usando ADO.NET
- Estructurar un proyecto siguiendo el patrón de capas (Models / Services / Views)
- Generar documentos PDF profesionales con QuestPDF
- Implementar navegación Flyout con múltiples páginas en Shell

---

## 2. 🛠️ El Stack Técnico Elite

> 💡 **Concepto clave: Stack Tecnológico**
> Un "stack" es el conjunto de tecnologías que trabajan en conjunto para construir una aplicación. Elegir el stack correcto determina el rendimiento, mantenibilidad y escalabilidad de tu sistema.

### Tecnologías Utilizadas

---

#### ![.NET](https://img.shields.io/badge/.NET_9.0-512BD4?style=flat&logo=dotnet&logoColor=white) .NET 9.0 SDK — *El Motor Multiplataforma*

**.NET 9.0** es el runtime de Microsoft que permite compilar y ejecutar aplicaciones en múltiples plataformas. Es la base sobre la que todo lo demás corre.

- ✅ Alto rendimiento y bajo consumo de memoria
- ✅ Soporte a largo plazo (LTS)
- ✅ Ecosistema masivo de librerías vía NuGet

---

#### ![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp&logoColor=white) C# — *El Lenguaje de la Lógica*

**C#** es el lenguaje de programación orientado a objetos que usamos para toda la lógica de negocio. Sus características clave en este proyecto:

- `async/await` → operaciones de base de datos sin bloquear la interfaz
- Tipado fuerte → detecta errores en compilación, no en ejecución
- LINQ → consultas a listas en memoria de forma elegante

---

#### ![XAML](https://img.shields.io/badge/XAML-.NET_MAUI-0078D4?style=flat&logo=xaml&logoColor=white) XAML + .NET MAUI — *La Interfaz Visual*

**XAML** (Extensible Application Markup Language) es el lenguaje declarativo para diseñar interfaces. En lugar de escribir código para crear botones, los declaras como etiquetas XML.

```xml
<!-- Así de simple es crear un botón en XAML -->
<Button Text="Guardar Venta" BackgroundColor="Green" Clicked="OnGuardarClicked"/>
```

**.NET MAUI** interpreta ese XAML y genera los controles nativos de cada plataforma (iOS usa UIButton, Android usa android.widget.Button, etc.).

---

#### ![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=flat&logo=microsoftsqlserver&logoColor=white) SQL Server + T-SQL — *El Almacén Inteligente*

**SQL Server** es el motor de base de datos relacional de Microsoft. Usamos **T-SQL** (Transact-SQL), su dialecto enriquecido que añade:

- `IDENTITY(1,1)` → auto-incremento de IDs
- Columnas `AS (...) PERSISTED` → cálculos automáticos guardados en disco
- `FOREIGN KEY` → integridad referencial entre tablas

---

#### ![SqlClient](https://img.shields.io/badge/Microsoft.Data.SqlClient-0078D4?style=flat&logo=microsoft&logoColor=white) Microsoft.Data.SqlClient — *El Puente de Conexión*

Esta librería NuGet es el **driver oficial** para conectar aplicaciones .NET a SQL Server. Implementa el protocolo TDS (Tabular Data Stream) para comunicación de ultra-baja latencia.

```
Instalar con NuGet: Microsoft.Data.SqlClient
```

---

#### ![QuestPDF](https://img.shields.io/badge/QuestPDF-FF6B35?style=flat&logo=adobeacrobatreader&logoColor=white) QuestPDF — *El Generador de Documentos*

**QuestPDF** es una librería open-source para generar PDFs en .NET mediante una **Fluent API** (API encadenada). Es la alternativa moderna a iTextSharp.

```csharp
// Así se ve su API fluida en acción:
container.Column(col => {
    col.Item().Text("Factura #001").Bold();
    col.Item().LineHorizontal(1);
});
```

```
Instalar con NuGet: QuestPDF
```

---

#### ![Shell](https://img.shields.io/badge/.NET_MAUI_Shell-512BD4?style=flat&logo=dotnet&logoColor=white) .NET MAUI Shell — *El Sistema de Navegación*

**Shell** es el sistema de navegación de alto nivel en MAUI. Define la estructura de la app (menú lateral, pestañas, rutas) de forma declarativa en un solo archivo XAML.

---

## 3. 🎯 Planteamiento del Problema y Solución JPV

### 😰 El Problema del Mundo Real

Las pequeñas y medianas empresas frecuentemente gestionan sus ventas mediante:

- 📊 Hojas de cálculo Excel con datos duplicados
- 📝 Cuadernos físicos sin respaldo
- 💻 Software costoso que no se adapta a su negocio

**Consecuencias:**
- No hay control de inventario en tiempo real
- Los cálculos de ITBIS se hacen manualmente (y con errores)
- No hay historial de ventas por cliente o vendedor
- No se pueden generar comprobantes físicos inmediatamente

### ✅ La Solución JPV Pro

```
                    ┌─────────────────────────────────┐
                    │         USUARIO FINAL           │
                    └────────────┬────────────────────┘
                                 │
                    ┌────────────▼────────────────────┐
                    │        CAPA DE VISTAS           │
                    │  MainPage | Inventario | Reporte│
                    └────────────┬────────────────────┘
                                 │
                    ┌────────────▼────────────────────┐
                    │       CAPA DE SERVICIOS         │
                    │   VentaService (lógica CRUD)    │
                    └────────────┬────────────────────┘
                                 │
                    ┌────────────▼────────────────────┐
                    │      CAPA DE BASE DE DATOS      │
                    │  SQL Server (5 tablas + ITBIS)  │
                    └─────────────────────────────────┘
```

Esta arquitectura de **3 capas** separa responsabilidades: la vista no sabe de SQL, y la base de datos no sabe de interfaz. Esto hace el código **mantenible**, **testeable** y **escalable**.

---

## 4. 🏗️ Estructura Completa del Proyecto

> 💡 **¿Por qué organizar el proyecto en carpetas?**
> Separar el código en carpetas por responsabilidad es una **buena práctica** llamada *Separation of Concerns*. Si en el futuro necesitas cambiar la base de datos de SQL Server a MySQL, solo tocas la carpeta `Services`, sin afectar las vistas.

```
ProyectoMauiCRUD_Copia/
│
│   App.xaml              ← 🎨 Estilos y recursos globales (colores, fuentes)
│   App.xaml.cs           ← 🚀 Punto de arranque: decide la primera página
│   AppShell.xaml         ← 🧭 Menú lateral (Flyout) y rutas de navegación
│   MauiProgram.cs        ← ⚙️ Inyección de dependencias y licencias
│   *.csproj              ← 📦 Manifiesto del proyecto (NuGet, plataformas)
│
├── 📁 Models/
│       Venta.cs          ← 🏗️ Clases C# que representan las entidades del negocio
│                            (Venta, Producto, Cliente, Vendedor, Categoria)
│
├── 📁 Services/
│       ConfigDB.cs       ← 🔑 Cadena de conexión a SQL Server (IP, BD, credenciales)
│       VentaService.cs   ← 🧠 CEREBRO: Toda la lógica de acceso a datos (CRUD + JOINs)
│
└── 📁 Views/
        MainPage.xaml         ← 🖥️ Interfaz de registro y gestión de Ventas
        MainPage.xaml.cs      ← ⚡ Lógica de ventas, validaciones y generación de PDF
        InventarioPage.xaml   ← 📦 Interfaz de gestión de Productos y Stock
        InventarioPage.xaml.cs← ⚡ CRUD completo de inventario
        ReportesPage.xaml     ← 📊 Interfaz de Inteligencia de Negocios
        ReportesPage.xaml.cs  ← ⚡ Filtros dinámicos, totales y exportación
```

### 📌 Regla de Oro de la Arquitectura

> Cada capa solo debe conocer a la capa inmediatamente inferior:
> - `Views` llama a `Services` ✅
> - `Views` llama directamente a la BD ❌
> - `Services` usa `Models` ✅
> - `Services` mezcla lógica de UI ❌

---

## 5. 🗄️ El Cimiento: Base de Datos SQL Server

> 💡 **¿Por qué SQL Server y no SQLite?**
> SQLite es ideal para apps móviles sin conexión. SQL Server es para **sistemas empresariales** donde múltiples usuarios (cajeros, supervisores) acceden simultáneamente a la misma base de datos centralizada en un servidor.

### Diagrama de Relaciones (ERD)

```
┌──────────────┐       ┌──────────────────────────────┐
│  Categoria   │       │           Venta              │
│──────────────│       │──────────────────────────────│
│ Id (PK)      │◄──┐   │ Id (PK)                      │
│ Nombre       │   │   │ Fecha                        │
└──────────────┘   │   │ ClienteId  (FK → Cliente)    │
                   │   │ VendedorId (FK → Vendedor)   │
┌──────────────┐   │   │ ProductoId (FK → Producto)   │
│   Producto   │   │   │ Cantidad                     │
│──────────────│   │   │ PrecioVentaAplicado           │
│ Id (PK)      │◄──┼───│ SubTotal  [CALCULADO]         │
│ Nombre       │   │   │ Itbis     [CALCULADO]         │
│ CategoriaId  │───┘   │ Total     [CALCULADO]         │
│ PrecioCompra │       └──────────────────────────────┘
│ PrecioVenta  │              │           │
│ Stock        │              │           │
└──────────────┘    ┌─────────┘  ┌────────┘
                    ▼            ▼
             ┌──────────┐  ┌──────────┐
             │ Cliente  │  │ Vendedor │
             │──────────│  │──────────│
             │ Id (PK)  │  │ Id (PK)  │
             │ Nombre   │  │ Nombre   │
             │ RNC      │  │ Codigo   │
             │ Telefono │  └──────────┘
             └──────────┘
```

### 📝 Script SQL Completo y Comentado

```sql
-- ============================================================
-- PASO 1: Crear la base de datos
-- El comando GO separa los "lotes" de instrucciones T-SQL.
-- Después de crear la BD, debemos indicarle a SQL Server que
-- la use con el comando USE.
-- ============================================================
CREATE DATABASE VentasDB_Relacional;
GO
USE VentasDB_Relacional;
GO

-- ============================================================
-- TABLA 1: Categoria
-- Tabla maestra simple. Solo Id y Nombre.
-- IDENTITY(1,1): el Id empieza en 1 y se incrementa de 1 en 1
-- automáticamente. No necesitas insertar el Id manualmente.
-- ============================================================
CREATE TABLE Categoria (
    Id     INT          IDENTITY(1,1) PRIMARY KEY,  -- Clave primaria auto-incremental
    Nombre VARCHAR(100) NOT NULL                     -- No permite valores nulos
);

-- ============================================================
-- TABLA 2: Producto
-- Tiene dos precios: Compra (costo) y Venta (lo que cobra al cliente).
-- Esto permite calcular el margen de ganancia en reportes.
-- CategoriaId es una FOREIGN KEY: garantiza que no puedas
-- insertar un producto con una categoría que no exista.
-- ============================================================
CREATE TABLE Producto (
    Id           INT           IDENTITY(1,1) PRIMARY KEY,
    Nombre       VARCHAR(100)  NOT NULL,
    CategoriaId  INT           NOT NULL,              -- Referencia a Categoria
    PrecioCompra DECIMAL(10,2) NOT NULL,              -- Costo del producto
    PrecioVenta  DECIMAL(10,2) NOT NULL,              -- Precio al público
    Stock        INT           NOT NULL,              -- Unidades disponibles

    -- Restricción de integridad referencial:
    -- Si intentas borrar una categoría con productos, SQL Server lo rechaza.
    CONSTRAINT FK_Producto_Categoria
        FOREIGN KEY (CategoriaId) REFERENCES Categoria(Id)
);

-- ============================================================
-- TABLA 3: Cliente
-- RNC (Registro Nacional del Contribuyente) es opcional,
-- por eso usamos VARCHAR sin NOT NULL (admite NULL).
-- ============================================================
CREATE TABLE Cliente (
    Id       INT          IDENTITY(1,1) PRIMARY KEY,
    Nombre   VARCHAR(100) NOT NULL,
    RNC      VARCHAR(20),          -- Opcional: puede ser NULL
    Telefono VARCHAR(20)           -- Opcional: puede ser NULL
);

-- ============================================================
-- TABLA 4: Vendedor
-- El campo Codigo es UNIQUE: no pueden existir dos vendedores
-- con el mismo código de empleado.
-- ============================================================
CREATE TABLE Vendedor (
    Id     INT          IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Codigo VARCHAR(20)  NOT NULL UNIQUE  -- Código único por vendedor
);

-- ============================================================
-- TABLA 5: Venta (La más importante y compleja)
--
-- COLUMNAS CALCULADAS (AS ... PERSISTED):
-- Son columnas virtuales cuyo valor SQL Server calcula
-- automáticamente basándose en otras columnas.
-- PERSISTED significa que el valor SE GUARDA en disco,
-- lo que acelera las consultas (no recalcula en cada SELECT).
--
-- SubTotal = Cantidad × PrecioVentaAplicado
-- Itbis    = SubTotal × 18% (impuesto dominicano)
-- Total    = SubTotal × 118% (precio con impuesto incluido)
--
-- Nota: PrecioVentaAplicado se guarda porque el precio del
-- producto podría cambiar en el futuro. Queremos saber qué
-- precio se cobró EN EL MOMENTO de la venta.
-- ============================================================
CREATE TABLE Venta (
    Id                   INT           IDENTITY(1,1) PRIMARY KEY,
    Fecha                DATETIME      NOT NULL DEFAULT GETDATE(), -- Se auto-completa con fecha/hora actual
    ClienteId            INT           NOT NULL,
    VendedorId           INT           NOT NULL,
    ProductoId           INT           NOT NULL,
    Cantidad             INT           NOT NULL,
    PrecioVentaAplicado  DECIMAL(10,2) NOT NULL,

    -- ↓ Columnas calculadas automáticamente por SQL Server ↓
    SubTotal AS (Cantidad * PrecioVentaAplicado)              PERSISTED,
    Itbis    AS ((Cantidad * PrecioVentaAplicado) * 0.18)     PERSISTED,
    Total    AS ((Cantidad * PrecioVentaAplicado) * 1.18)     PERSISTED,

    -- Claves foráneas que garantizan consistencia de datos
    CONSTRAINT FK_Venta_Cliente   FOREIGN KEY (ClienteId)   REFERENCES Cliente(Id),
    CONSTRAINT FK_Venta_Vendedor  FOREIGN KEY (VendedorId)  REFERENCES Vendedor(Id),
    CONSTRAINT FK_Venta_Producto  FOREIGN KEY (ProductoId)  REFERENCES Producto(Id)
);
GO
```

> ⚠️ **Importante antes de correr el script:**
> Abre **SQL Server Management Studio (SSMS)** o **Azure Data Studio**, conéctate a tu instancia de SQL Server, y ejecuta este script completo. Asegúrate de que no exista previamente una base de datos llamada `VentasDB_Relacional`.

---

## 6. 💻 Código Fuente Comentado Línea a Línea

### 6.1 Los Modelos (`Models/Venta.cs`)

> 💡 **¿Qué es un Modelo?**
> Un **modelo** (también llamado *entidad* o *POCO — Plain Old C# Object*) es una clase C# que representa la estructura de datos de tu negocio. Es el "contrato" entre la base de datos y el resto de la aplicación. Cada propiedad de la clase corresponde a una columna de la tabla en SQL Server.

```csharp
// Directiva: importamos el namespace System para usar DateTime
using System;

// Declaramos el namespace del proyecto para que otras clases
// puedan referenciar estos modelos con "using ProyectoMauiCRUD.Models"
namespace ProyectoMauiCRUD.Models
{
    // =========================================================
    // MODELO: Categoria
    // Mapea directamente con la tabla [Categoria] en SQL Server.
    // Es el modelo más simple: solo Id y Nombre.
    // =========================================================
    public class Categoria
    {
        public int Id { get; set; }                    // Corresponde a Id INT en SQL
        public string Nombre { get; set; } = string.Empty;  // Valor por defecto vacío
    }

    // =========================================================
    // MODELO: Producto
    // Mapea con la tabla [Producto].
    // Nota: CategoriaNombre NO existe en la tabla — se obtiene
    // mediante JOIN en el servicio. El "?" indica que puede ser null.
    // =========================================================
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }           // FK: Id de la categoría
        public string? CategoriaNombre { get; set; }   // Viene del JOIN (puede ser null)
        public decimal PrecioCompra { get; set; }      // decimal: ideal para dinero (evita errores de punto flotante)
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
    }

    // =========================================================
    // MODELO: Cliente
    // Los campos RNC y Telefono son opcionales en la BD (nullable),
    // por eso se declaran como string? (nullable reference type).
    // =========================================================
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? RNC { get; set; }       // Nullable: puede no tener RNC
        public string? Telefono { get; set; }  // Nullable: puede no tener teléfono
    }

    // =========================================================
    // MODELO: Vendedor
    // El Codigo es único en la BD, pero aquí es solo una propiedad.
    // La unicidad la controla SQL Server con la restricción UNIQUE.
    // =========================================================
    public class Vendedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }

    // =========================================================
    // MODELO: Venta (el más completo)
    //
    // Importante: SubTotal, Itbis y Total son calculados por
    // SQL Server. Aquí los incluimos como propiedades de solo
    // lectura (en la práctica) para mostrarlos en la UI.
    //
    // ClienteNombre, VendedorNombre y ProductoNombre NO existen
    // en la tabla Venta. Se obtienen con INNER JOINs y se
    // populan al leer. Esto evita que la UI muestre solo IDs.
    // =========================================================
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }             // Fecha y hora de la venta

        // IDs para inserción/actualización (lo que guarda SQL)
        public int ClienteId { get; set; }
        public int VendedorId { get; set; }
        public int ProductoId { get; set; }

        // Nombres para mostrar en la UI (vienen del JOIN)
        public string ClienteNombre { get; set; } = string.Empty;
        public string VendedorNombre { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public decimal PrecioVenta { get; set; }    // El precio aplicado al momento

        // Estos 3 valores los calcula SQL Server automáticamente
        public decimal SubTotal { get; set; }
        public decimal Itbis { get; set; }
        public decimal Total { get; set; }
    }
}
```

---

### 6.2 La Configuración de Conexión (`Services/ConfigDB.cs`)

> 💡 **¿Qué es una Cadena de Conexión?**
> Es un string que contiene todos los parámetros necesarios para que la aplicación encuentre y se autentique con la base de datos: la dirección IP del servidor, el nombre de la BD, usuario y contraseña.

```csharp
namespace ProyectoMauiCRUD.Services
{
    // Clase estática: no necesita instanciarse (new ConfigDB()).
    // Se accede directamente como ConfigDB.ConnectionString
    public static class ConfigDB
    {
        // La cadena de conexión tiene varios componentes:
        //
        // Server=10.0.0.15         → IP del servidor SQL Server en tu red local
        //                            En desarrollo local usa: Server=localhost o Server=.\SQLEXPRESS
        //
        // Database=VentasDB_Relacional → Nombre exacto de la base de datos
        //
        // User Id=sa               → Usuario SQL (sa = System Administrator)
        //                            ⚠️ En producción usa un usuario con permisos mínimos
        //
        // Password=TuPassword      → Contraseña del usuario SQL
        //
        // TrustServerCertificate=True → Para conexiones locales sin certificado SSL válido
        //                               En producción configura un certificado real
        //
        // Encrypt=False            → Desactiva cifrado TLS para redes locales de confianza
        //                            En producción activa esto con Encrypt=True
        public static string ConnectionString =>
            "Server=10.0.0.15;" +
            "Database=VentasDB_Relacional;" +
            "User Id=sa;" +
            "Password=TuPassword;" +
            "TrustServerCertificate=True;" +
            "Encrypt=False;";
    }
}
```

> 🔐 **Buenas Prácticas de Seguridad:**
> En una aplicación real, **NUNCA** hardcodees las credenciales en el código fuente. Usa:
> - Variables de entorno del sistema operativo
> - Archivos `appsettings.json` excluidos del control de versiones via `.gitignore`
> - Azure Key Vault para entornos cloud

---

### 6.3 La Navegación (`AppShell.xaml`)

> 💡 **¿Qué es Shell en .NET MAUI?**
> Shell es el contenedor de navegación de alto nivel. Define la jerarquía de páginas, los menús y las rutas URI de la app. Con `FlyoutBehavior="Flyout"` activas el menú lateral deslizable (como el cajón de navegación de Gmail).

```xml
<!-- Shell es el contenedor raíz de toda la aplicación.
     x:Class vincula este XAML con la clase AppShell en C# -->
<Shell x:Class="ProyectoMauiCRUD.AppShell"
       xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:ProyectoMauiCRUD.Views"
       Shell.FlyoutBehavior="Flyout"
       Title="JPV PRO V2">
    <!--
        FlyoutBehavior="Flyout" → activa el menú lateral deslizante.
        Alternativas: "Disabled" (sin menú), "Locked" (siempre visible)

        xmlns:views declara el alias "views:" para acceder a las
        clases en el namespace ProyectoMauiCRUD.Views
    -->

    <!-- FlyoutItem: cada ítem que aparece en el menú lateral.
         Title → el texto que el usuario ve en el menú
         Icon  → la imagen del ícono (debe estar en Resources/Images) -->
    <FlyoutItem Title="💰 Ventas" Icon="dotnet_bot.png">
        <!--
            ShellContent: define qué página se muestra.
            ContentTemplate usa DataTemplate para crear la página
            de forma "lazy" (solo cuando el usuario la visita),
            lo que mejora el rendimiento al arrancar.
        -->
        <ShellContent ContentTemplate="{DataTemplate views:MainPage}" />
    </FlyoutItem>

    <FlyoutItem Title="📦 Inventario" Icon="dotnet_bot.png">
        <ShellContent ContentTemplate="{DataTemplate views:InventarioPage}" />
    </FlyoutItem>

    <FlyoutItem Title="📊 Reportes" Icon="dotnet_bot.png">
        <ShellContent ContentTemplate="{DataTemplate views:ReportesPage}" />
    </FlyoutItem>

</Shell>
```

---

### 6.4 Servicio de Datos Maestro (`Services/VentaService.cs`)

> 💡 **¿Por qué usar un Service?**
> Un Service centraliza toda la lógica de acceso a datos. Si mañana cambias de SQL Server a MySQL, solo modificas este archivo. Las vistas no necesitan saber *cómo* se obtienen los datos, solo los usan.

> 💡 **¿Qué es `async/await`?**
> Las operaciones de base de datos pueden tardar milisegundos o segundos. Sin `async/await`, la UI se congelaría mientras espera la respuesta. Con `async/await`, la UI sigue respondiendo mientras la consulta se ejecuta en segundo plano.

```csharp
using Microsoft.Data.SqlClient;  // Driver oficial de conexión a SQL Server
using ProyectoMauiCRUD.Models;   // Nuestros modelos (Venta, Producto, etc.)
using System.Collections.Generic;
using System.Threading.Tasks;    // Necesario para Task<T> y async/await

namespace ProyectoMauiCRUD.Services
{
    // Esta clase es el CEREBRO del sistema.
    // Contiene todos los métodos para leer y escribir en la BD.
    public class VentaService
    {
        // La cadena de conexión se obtiene de ConfigDB.
        // "readonly" significa que no puede cambiar después de asignarse.
        private readonly string connectionString = ConfigDB.ConnectionString;

        // =============================================================
        // MÉTODO: GetClientes()
        // Lee todos los clientes de la tabla Cliente.
        //
        // Retorna: Task<List<Cliente>> → una lista asíncrona de clientes
        //
        // Patrón ADO.NET:
        //   1. Abrir conexión    (SqlConnection)
        //   2. Crear comando     (SqlCommand)
        //   3. Ejecutar query    (ExecuteReaderAsync)
        //   4. Leer filas        (reader.ReadAsync)
        //   5. Mapear a modelo   (new Cliente { ... })
        //   6. Cerrar conexión   (automático con "using")
        // =============================================================
        public async Task<List<Cliente>> GetClientes()
        {
            var lista = new List<Cliente>();

            // "using" garantiza que la conexión se cierre y libere
            // recursos aunque ocurra una excepción.
            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync(); // Abrir conexión de forma asíncrona

            // SqlCommand: encapsula la consulta SQL
            using var cmd = new SqlCommand("SELECT Id, Nombre, RNC, Telefono FROM Cliente ORDER BY Nombre", conexion);

            // ExecuteReaderAsync: ejecuta el SELECT y retorna un "cursor" de filas
            using var reader = await cmd.ExecuteReaderAsync();

            // reader.ReadAsync() avanza a la siguiente fila. Retorna false cuando no hay más.
            while (await reader.ReadAsync())
            {
                lista.Add(new Cliente
                {
                    // GetInt32(0) lee la columna en posición 0 (Id)
                    Id = reader.GetInt32(0),
                    // GetString(1) lee la columna en posición 1 (Nombre)
                    Nombre = reader.GetString(1),
                    // IsDBNull verifica si el valor es NULL antes de leerlo
                    RNC = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Telefono = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return lista;
        }

        // =============================================================
        // MÉTODO: GetVendedores()
        // Mismo patrón que GetClientes pero para la tabla Vendedor.
        // =============================================================
        public async Task<List<Vendedor>> GetVendedores()
        {
            var lista = new List<Vendedor>();

            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync();

            using var cmd = new SqlCommand("SELECT Id, Nombre, Codigo FROM Vendedor ORDER BY Nombre", conexion);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new Vendedor
                {
                    Id     = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Codigo = reader.GetString(2)
                });
            }

            return lista;
        }

        // =============================================================
        // MÉTODO: GetCategorias()
        // =============================================================
        public async Task<List<Categoria>> GetCategorias()
        {
            var lista = new List<Categoria>();

            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync();

            using var cmd = new SqlCommand("SELECT Id, Nombre FROM Categoria ORDER BY Nombre", conexion);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new Categoria
                {
                    Id     = reader.GetInt32(0),
                    Nombre = reader.GetString(1)
                });
            }

            return lista;
        }

        // =============================================================
        // MÉTODO: GetProductos()
        // Usa INNER JOIN para traer el nombre de la categoría.
        //
        // Sin JOIN veríamos: Id=1, Nombre="Coca Cola", CategoriaId=3
        // Con JOIN vemos:    Id=1, Nombre="Coca Cola", CategoriaNombre="Bebidas"
        //
        // Esto es mucho más útil para mostrar en la UI.
        // =============================================================
        public async Task<List<Producto>> GetProductos()
        {
            var lista = new List<Producto>();

            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync();

            // INNER JOIN: solo devuelve productos que tienen una categoría válida.
            // p.* = todas las columnas de Producto
            // c.Nombre AS CategoriaNombre = alias para no confundir con p.Nombre
            var sql = @"
                SELECT p.Id, p.Nombre, p.CategoriaId, c.Nombre AS CategoriaNombre,
                       p.PrecioCompra, p.PrecioVenta, p.Stock
                FROM   Producto p
                INNER JOIN Categoria c ON p.CategoriaId = c.Id
                ORDER BY p.Nombre";

            using var cmd    = new SqlCommand(sql, conexion);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new Producto
                {
                    Id              = reader.GetInt32(0),
                    Nombre          = reader.GetString(1),
                    CategoriaId     = reader.GetInt32(2),
                    CategoriaNombre = reader.GetString(3),  // Del JOIN
                    PrecioCompra    = reader.GetDecimal(4),
                    PrecioVenta     = reader.GetDecimal(5),
                    Stock           = reader.GetInt32(6)
                });
            }

            return lista;
        }

        // =============================================================
        // MÉTODO: GetReporteVentas(cId, vId, pId)
        // El método más complejo: usa TRIPLE INNER JOIN y filtros dinámicos.
        //
        // Parámetros opcionales (int? = nullable int):
        //   cId → filtrar por cliente (null = todos los clientes)
        //   vId → filtrar por vendedor (null = todos los vendedores)
        //   pId → filtrar por producto (null = todos los productos)
        //
        // Filtros dinámicos con @parametros SQL:
        // Usamos parámetros (@ClienteId) en lugar de concatenar strings
        // para PREVENIR inyección SQL (SQL Injection).
        // =============================================================
        public async Task<List<Venta>> GetReporteVentas(int? cId, int? vId, int? pId)
        {
            var lista = new List<Venta>();

            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync();

            // Triple JOIN: une 4 tablas en una sola consulta
            // WHERE 1=1: truco para agregar condiciones AND dinámicamente
            var sql = @"
                SELECT v.Id, v.Fecha,
                       v.ClienteId,  c.Nombre AS ClienteNombre,
                       v.VendedorId, ve.Nombre AS VendedorNombre,
                       v.ProductoId, p.Nombre  AS ProductoNombre,
                       v.Cantidad, v.PrecioVentaAplicado,
                       v.SubTotal, v.Itbis, v.Total
                FROM   Venta v
                INNER JOIN Cliente  c  ON v.ClienteId  = c.Id
                INNER JOIN Vendedor ve ON v.VendedorId = ve.Id
                INNER JOIN Producto p  ON v.ProductoId = p.Id
                WHERE 1=1
                  AND (@ClienteId  IS NULL OR v.ClienteId  = @ClienteId)
                  AND (@VendedorId IS NULL OR v.VendedorId = @VendedorId)
                  AND (@ProductoId IS NULL OR v.ProductoId = @ProductoId)
                ORDER BY v.Fecha DESC";

            using var cmd = new SqlCommand(sql, conexion);

            // AddWithValue: agrega parámetros al comando.
            // Si el int? es null, pasamos DBNull.Value (equivalente a NULL en SQL).
            // Esto es SEGURO contra SQL Injection (nunca concatenes strings con datos del usuario).
            cmd.Parameters.AddWithValue("@ClienteId",  (object?)cId  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VendedorId", (object?)vId  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProductoId", (object?)pId  ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new Venta
                {
                    Id             = reader.GetInt32(0),
                    Fecha          = reader.GetDateTime(1),
                    ClienteId      = reader.GetInt32(2),
                    ClienteNombre  = reader.GetString(3),   // Del JOIN con Cliente
                    VendedorId     = reader.GetInt32(4),
                    VendedorNombre = reader.GetString(5),   // Del JOIN con Vendedor
                    ProductoId     = reader.GetInt32(6),
                    ProductoNombre = reader.GetString(7),   // Del JOIN con Producto
                    Cantidad       = reader.GetInt32(8),
                    PrecioVenta    = reader.GetDecimal(9),
                    SubTotal       = reader.GetDecimal(10), // Calculado por SQL Server
                    Itbis          = reader.GetDecimal(11), // Calculado por SQL Server
                    Total          = reader.GetDecimal(12)  // Calculado por SQL Server
                });
            }

            return lista;
        }

        // =============================================================
        // MÉTODO: InsertVenta(venta)
        // Inserta una nueva venta en la base de datos.
        //
        // IMPORTANTE: NO insertamos SubTotal, Itbis ni Total porque
        // son columnas calculadas — SQL Server las calcula solo.
        // Solo insertamos los datos "reales" de la venta.
        //
        // También actualiza el stock del producto (resta la cantidad vendida).
        // =============================================================
        public async Task InsertVenta(Venta v)
        {
            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync();

            // Usamos una TRANSACCIÓN para garantizar que si la inserción
            // de la venta falla, el stock NO se descuente (atomicidad).
            using var transaccion = conexion.BeginTransaction();

            try
            {
                // Paso 1: Insertar la venta
                var sqlInsert = @"
                    INSERT INTO Venta (ClienteId, VendedorId, ProductoId, Cantidad, PrecioVentaAplicado)
                    VALUES (@ClienteId, @VendedorId, @ProductoId, @Cantidad, @Precio)";

                using var cmdInsert = new SqlCommand(sqlInsert, conexion, transaccion);
                cmdInsert.Parameters.AddWithValue("@ClienteId",  v.ClienteId);
                cmdInsert.Parameters.AddWithValue("@VendedorId", v.VendedorId);
                cmdInsert.Parameters.AddWithValue("@ProductoId", v.ProductoId);
                cmdInsert.Parameters.AddWithValue("@Cantidad",   v.Cantidad);
                cmdInsert.Parameters.AddWithValue("@Precio",     v.PrecioVenta);

                // ExecuteNonQueryAsync: para INSERT/UPDATE/DELETE (no retorna filas)
                await cmdInsert.ExecuteNonQueryAsync();

                // Paso 2: Descontar el stock del producto
                var sqlStock = "UPDATE Producto SET Stock = Stock - @Cantidad WHERE Id = @ProductoId";

                using var cmdStock = new SqlCommand(sqlStock, conexion, transaccion);
                cmdStock.Parameters.AddWithValue("@Cantidad",   v.Cantidad);
                cmdStock.Parameters.AddWithValue("@ProductoId", v.ProductoId);

                await cmdStock.ExecuteNonQueryAsync();

                // Confirmar ambas operaciones como una unidad
                await transaccion.CommitAsync();
            }
            catch
            {
                // Si algo falla, revertir todo (ni la venta ni el stock cambian)
                await transaccion.RollbackAsync();
                throw; // Re-lanzar la excepción para que la UI la maneje
            }
        }

        // =============================================================
        // MÉTODO: UpdateVenta(venta)
        // Actualiza una venta existente por su Id.
        // Solo se pueden cambiar los campos editables (no los calculados).
        // =============================================================
        public async Task UpdateVenta(Venta v)
        {
            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync();

            var sql = @"
                UPDATE Venta
                SET ClienteId           = @ClienteId,
                    VendedorId          = @VendedorId,
                    ProductoId          = @ProductoId,
                    Cantidad            = @Cantidad,
                    PrecioVentaAplicado = @Precio
                WHERE Id = @Id";  -- Crucial: sin WHERE actualizaría TODAS las ventas

            using var cmd = new SqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("@ClienteId",  v.ClienteId);
            cmd.Parameters.AddWithValue("@VendedorId", v.VendedorId);
            cmd.Parameters.AddWithValue("@ProductoId", v.ProductoId);
            cmd.Parameters.AddWithValue("@Cantidad",   v.Cantidad);
            cmd.Parameters.AddWithValue("@Precio",     v.PrecioVenta);
            cmd.Parameters.AddWithValue("@Id",         v.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        // =============================================================
        // MÉTODO: DeleteVenta(id)
        // Elimina una venta por su Id.
        // =============================================================
        public async Task DeleteVenta(int id)
        {
            using var conexion = new SqlConnection(connectionString);
            await conexion.OpenAsync();

            using var cmd = new SqlCommand("DELETE FROM Venta WHERE Id = @Id", conexion);
            cmd.Parameters.AddWithValue("@Id", id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
```

---

## 7. 📄 Generación del Ticket PDF con QuestPDF

> 💡 **¿Por qué QuestPDF y no iTextSharp?**
> iTextSharp tiene una API imperativa y compleja. QuestPDF usa una **Fluent API** (encadenada) que se lee casi como prosa, y es completamente gratuita para proyectos menores a $1M de ingresos.

### Estructura del Ticket de 80mm

```
┌─────────────────────────┐  ← Ancho: 80mm (impresora térmica estándar)
│      JPV PRO V2.0       │
│   RNC: 101-12345-6      │
│  Av. Principal #123     │
│  Tel: 829-555-0000      │
├─────────────────────────┤
│ Fecha: 05/04/2026 10:30 │
│ Ticket: #00045          │
│ Cliente: Juan Pérez      │
│ Vendedor: María López   │
├─────────────────────────┤
│ DESCRIPCIÓN   CANT  TOT │
│ Coca Cola 2L   x2  $280 │
│ Pan de agua    x5   $75 │
├─────────────────────────┤
│         SubTotal: $355  │
│         ITBIS 18%:  $64 │
│         TOTAL:    $419  │
├─────────────────────────┤
│    ¡GRACIAS POR SU      │
│        COMPRA!          │
└─────────────────────────┘
```

### Código QuestPDF Comentado

```csharp
using QuestPDF.Fluent;      // Métodos de extensión fluidos (Column, Row, Text, etc.)
using QuestPDF.Helpers;     // Constantes como Colors, Units
using QuestPDF.Infrastructure; // Tipos base (Document, IDocument, etc.)

// La clase implementa IDocument: contrato que QuestPDF requiere
public class TicketDocument : IDocument
{
    // Recibimos la venta con todos sus datos (incluyendo nombres del JOIN)
    private readonly Venta _venta;

    public TicketDocument(Venta venta) => _venta = venta;

    // GetMetadata: configura propiedades del PDF (autor, título, tamaño)
    public DocumentMetadata GetMetadata() => new DocumentMetadata
    {
        Title = $"Ticket #{_venta.Id:D5}",  // D5 → formato 00045
        Author = "JPV Pro V2.0"
    };

    // Compose: aquí se define TODO el contenido del PDF
    // El parámetro "container" es el lienzo en blanco del documento
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            // Tamaño personalizado: 80mm de ancho, altura automática
            // Unit.Millimetre: usamos milímetros para precisión
            page.Size(80, PageSizes.A4.Height, Unit.Millimetre);
            page.Margin(3, Unit.Millimetre);   // Márgenes de 3mm en todos los lados
            page.DefaultTextStyle(x => x.FontSize(8)); // Fuente base del ticket

            // El contenido va en el cuerpo de la página
            page.Content().Column(col =>
            {
                // ── ENCABEZADO ──────────────────────────────────────────
                // .AlignCenter() → centra horizontalmente
                // .Text() con lambda permite aplicar múltiples estilos
                col.Item().AlignCenter().Text("JPV PRO V2.0")
                   .Bold().FontSize(12);

                col.Item().AlignCenter().Text("RNC: 101-12345-6");
                col.Item().AlignCenter().Text("Av. Principal #123, Santo Domingo");
                col.Item().AlignCenter().Text("Tel: 829-555-0000");

                // LineHorizontal: dibuja una línea separadora
                // El número es el grosor en puntos
                col.Item().PaddingVertical(2).LineHorizontal(0.5f);

                // ── DATOS DE LA VENTA ────────────────────────────────────
                // .Row() crea una fila de dos columnas (clave: valor)
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Fecha: {_venta.Fecha:dd/MM/yyyy HH:mm}");
                    row.RelativeItem().AlignRight().Text($"Ticket #{_venta.Id:D5}");
                });

                col.Item().Text($"Cliente:  {_venta.ClienteNombre}");
                col.Item().Text($"Vendedor: {_venta.VendedorNombre}");

                col.Item().PaddingVertical(2).LineHorizontal(0.5f);

                // ── ENCABEZADO DE TABLA ──────────────────────────────────
                col.Item().Row(row =>
                {
                    row.RelativeItem(5).Text("DESCRIPCIÓN").Bold();
                    row.RelativeItem(1).AlignCenter().Text("CANT").Bold();
                    row.RelativeItem(2).AlignRight().Text("TOTAL").Bold();
                });

                col.Item().LineHorizontal(0.5f);

                // ── LÍNEA DE PRODUCTO ────────────────────────────────────
                // RelativeItem(5): esta columna ocupa 5/8 del ancho
                // RelativeItem(1): ocupa 1/8
                // RelativeItem(2): ocupa 2/8
                col.Item().Row(row =>
                {
                    row.RelativeItem(5).Text(_venta.ProductoNombre);
                    row.RelativeItem(1).AlignCenter().Text(_venta.Cantidad.ToString());
                    row.RelativeItem(2).AlignRight().Text($"RD${_venta.Total:N2}");
                });

                col.Item().PaddingVertical(2).LineHorizontal(0.5f);

                // ── TOTALES FINANCIEROS ──────────────────────────────────
                // Cada fila tiene etiqueta a la izquierda y monto a la derecha
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("SubTotal:");
                    row.ConstantItem(45).AlignRight().Text($"RD${_venta.SubTotal:N2}");
                });

                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("ITBIS 18%:");
                    row.ConstantItem(45).AlignRight().Text($"RD${_venta.Itbis:N2}");
                });

                // El total final va en negrita y mayor tamaño
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("TOTAL:").Bold().FontSize(10);
                    row.ConstantItem(45).AlignRight()
                       .Text($"RD${_venta.Total:N2}").Bold().FontSize(10);
                });

                col.Item().PaddingVertical(3).LineHorizontal(0.5f);

                // ── PIE DE PÁGINA ────────────────────────────────────────
                col.Item().AlignCenter().Text("¡GRACIAS POR SU COMPRA!").Bold();
                col.Item().AlignCenter().Text("Conserve su comprobante");
            });
        });
    }
}

// ── CÓMO USAR ESTA CLASE EN MainPage.xaml.cs ────────────────────────────────
// Llama a este código cuando el usuario presione el botón "Imprimir Ticket":
//
// private async void OnImprimirTicketClicked(object sender, EventArgs e)
// {
//     if (_ventaSeleccionada == null) return;
//
//     // Definir la ruta del archivo PDF temporal
//     var rutaPdf = Path.Combine(FileSystem.CacheDirectory, $"ticket_{_ventaSeleccionada.Id}.pdf");
//
//     // Generar el PDF con QuestPDF
//     Document.Create(container =>
//     {
//         var ticket = new TicketDocument(_ventaSeleccionada);
//         ticket.Compose(container);
//     }).GeneratePdf(rutaPdf);
//
//     // Abrir el PDF con la aplicación predeterminada del dispositivo
//     await Launcher.OpenAsync(new OpenFileRequest
//     {
//         File = new ReadOnlyFile(rutaPdf)
//     });
// }
```

---

## 8. ⚙️ Registro de Servicios (`MauiProgram.cs`)

> 💡 **¿Qué es la Inyección de Dependencias?**
> En lugar de que cada vista cree su propio `new VentaService()`, registramos el servicio una vez en `MauiProgram.cs`. MAUI lo crea y lo "inyecta" donde se necesite. Esto facilita las pruebas unitarias y reduce el acoplamiento.

```csharp
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;   // Necesario para la licencia de QuestPDF
using ProyectoMauiCRUD.Services;
using ProyectoMauiCRUD.Views;

namespace ProyectoMauiCRUD
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    // Registramos fuentes personalizadas.
                    // Los archivos .ttf deben estar en Resources/Fonts/
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ── LICENCIA DE QUESTPDF ──────────────────────────────────────
            // QuestPDF requiere declarar el tipo de licencia antes de usarse.
            // Community: gratuita para proyectos menores a $1M USD de ingresos.
            QuestPDF.Settings.License = LicenseType.Community;

            // ── REGISTRO DE SERVICIOS (Inyección de Dependencias) ─────────
            // AddSingleton: una sola instancia compartida en toda la app.
            // Ideal para servicios de datos que no tienen estado variable.
            builder.Services.AddSingleton<VentaService>();

            // AddTransient: una nueva instancia cada vez que se necesite.
            // Ideal para páginas (cada visita es una página "fresca").
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<InventarioPage>();
            builder.Services.AddTransient<ReportesPage>();

#if DEBUG
            // Solo en modo Debug: logs detallados en la consola
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
```

---

## 9. 🚀 Conclusión y Próximos Pasos

### ✅ Lo que has construido

Al completar este manual, tienes un sistema que implementa:

- **Arquitectura de 3 capas** (Models / Services / Views) con separación clara de responsabilidades
- **Base de datos relacional** con 5 tablas, claves foráneas y columnas calculadas persistidas
- **Acceso a datos profesional** con ADO.NET, parámetros SQL seguros y transacciones
- **Operaciones asíncronas** con `async/await` para una UI siempre responsiva
- **Filtros dinámicos** con parámetros opcionales (nullable) en SQL
- **Generación de PDF** con QuestPDF para tickets de 80mm
- **Navegación Flyout** con Shell para múltiples secciones

### 🔮 Próximas Mejoras Sugeridas

| Mejora | Beneficio |
|---|---|
| 🔐 Autenticación de usuarios | Roles: administrador, cajero, solo lectura |
| 📱 Modo offline con SQLite local | Funciona sin red y sincroniza al reconectarse |
| 📈 Dashboard con gráficas | Visualización de ventas con LiveCharts2 |
| 🔔 Alertas de stock bajo | Notificación cuando el stock baja del mínimo |
| 🧪 Unit Tests con xUnit | Pruebas automatizadas del VentaService |
| 🌐 API REST con ASP.NET Core | Multi-usuario simultáneo desde la nube |

---

> 👨‍💻 **Ing. Juancito Peña**
> Sistema JPV Pro V2.0 — Construido con ❤️ en .NET MAUI
>
> *"El código no solo debe funcionar. Debe poder ser leído, entendido y mantenido."*
