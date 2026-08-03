# 📘 PLAN GENERAL MAESTRO: SISTEMA ERP Y CONTROL DE ACCESO (.NET MAUI + SQL SERVER + API REST)

## 🌟 INTRODUCCIÓN GENERAL AL PROYECTO Y SU RAZÓN DE SER

En el desarrollo de software moderno, construir aplicaciones robustas, escalables y seguras es un reto que requiere separar correctamente las responsabilidades. Este proyecto nace de la necesidad imperativa de crear un **Sistema Mini ERP y Punto de Venta (POS)** de nivel empresarial, con control de acceso avanzado, que funcione tanto en entornos móviles como de escritorio de manera impecable.

Para lograr un rendimiento óptimo y evitar sobrecargar los dispositivos de los usuarios, hemos diseñado una **Arquitectura Limpia en Tres Capas (Frontend, Backend y Pruebas)**. La justificación de esto es simple: un teléfono móvil no debería encargarse de tareas intensivas de procesamiento como generar un archivo PDF pesado o procesar grandes volúmenes de reportes SQL. Esas tareas deben ser delegadas a un servidor (API). 

### 🏗️ Desglose de los Proyectos de la Solución:

1. **CRUD_LOGIN_MAUI (Frontend Móvil/Escritorio):**
   * **Razón de ser:** Es el punto de interacción con el usuario final. Construido con .NET MAUI, permite que con un solo código base tengamos una aplicación nativa para Windows, Android e iOS.
   * **Mejora pedagógica:** Aquí el estudiante aprenderá el patrón MVVM y la gestión de interfaces UI responsivas. La app solo se encarga de mostrar datos, capturar eventos y solicitar acciones; nunca procesa lógica pesada, garantizando fluidez visual.

2. **CRUD_LOGIN_MAUI.Api (Backend de Microservicios):**
   * **Razón de ser:** Actúa como el "cerebro pesado" de nuestra arquitectura. Es una API RESTful construida con ASP.NET Core cuya responsabilidad principal en esta etapa es la generación estructurada y rápida de reportes en PDF utilizando librerías robustas (iText7 / QuestPDF).
   * **Mejora pedagógica:** El alumno comprenderá la importancia de los microservicios. Aprenderá cómo la aplicación MAUI hace una petición HTTP a esta API, la cual procesa la creación térmica del ticket de 80mm y devuelve el documento listo, sin agotar la batería ni la memoria del cliente.

3. **CRUD_LOGIN_MAUI.Tests (Aseguramiento de Calidad):**
   * **Razón de ser:** El software empresarial no puede permitirse fallar en producción. Este proyecto integra xUnit y Moq para realizar pruebas unitarias e integrales (End-to-End).
   * **Mejora pedagógica:** Se instruirá al estudiante sobre la cultura del Testing (TDD). Probar la base de datos y la generación de servicios antes de desplegar, es la marca de un ingeniero Senior.

---

## 🗺️ ESTRUCTURA COMPLETA IMPLEMENTADA

Esta es la radiografía exacta de los directorios y archivos que se han implementado y asegurado en todo el ecosistema.

### FASE 1: Preparación del Entorno y Base de Datos
*   **Archivos:** `LoginRolesDB_cif_MINI_ERP.sql` (Script Maestro).
*   **Objetivo:** Desplegar en SQL Server la base de datos `LoginRolesDB_cif`.
*   **Tablas:** Roles, Usuarios (Hash SHA2_256), Categoria, Producto, Ventas, Detalle_Ventas y Vistas de Reportes.

### FASE 2: Proyecto Backend (CRUD_LOGIN_MAUI.Api)
*   **Archivos Claves Creados:**
    *   `Program.cs` (Punto de arranque y configuración de controladores).
    *   `Controllers/PdfController.cs` (Endpoint POST `/api/pdf/ticket`).
    *   `Models/TicketRequest.cs` (Contrato de datos que recibe desde MAUI).
    *   `Services/TicketPdfGenerator.cs` (Lógica central de QuestPDF dibujando recibos térmicos 80mm).

### FASE 3: Proyecto Frontend (CRUD_LOGIN_MAUI)
*   **Configuración y Enrutamiento:**
    *   `AppShell.xaml` y `AppShell.xaml.cs` (Registro de todas las rutas y flyouts).
*   **Modelos de Datos (Models/):**
    *   (Implementados previamente según el manual)
*   **Capa de Negocios y BD (Services/):**
    *   `ConfigDB.cs` (Centralización estricta de la ConnectionString apuntando a 10.0.0.15).
    *   `VentaService.cs` (CRUD, Reportes Históricos y Transacciones ACID).
    *   `TicketPdfService.cs` (Servicio de consumo y creación del PDF en MAUI).
*   **Interfaces de Usuario UI (Views/):**
    *   `MainPage.xaml / .cs` (Login y enrutamiento por roles).
    *   `AdminPage.xaml / .cs` (Panel Maestro CRUD Usuarios).
    *   `RolesPage.xaml / .cs` (Panel Maestro CRUD Roles).
    *   `InventarioPage.xaml / .cs` (Gestión de Productos con validación de relaciones).
    *   `AlmacenistaPage.xaml / .cs` (Dashboard de métricas visuales y rotación de inventario).
    *   `VendedorPage.xaml / .cs` (Punto de Venta POS con carrito de compras y generación térmica).
    *   `SupervisorPage.xaml / .cs` (Panel administrativo de solo lectura).
