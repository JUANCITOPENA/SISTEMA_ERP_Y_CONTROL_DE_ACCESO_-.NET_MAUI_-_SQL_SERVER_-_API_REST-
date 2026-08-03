# 📋 PLAN DE TRABAJO: Integración Sistema Login/Roles + ERP de Ventas

## 🎯 Objetivo Principal
Fusionar el proyecto base de autenticación y roles con el sistema de ventas y facturación, creando una arquitectura limpia de tres capas (Models, Services, Views) y escalando la base de datos para soportar ventas de múltiples productos (Maestro-Detalle).

---

## 🏗️ 1. Arquitectura y Estructura del Proyecto
Vamos a reorganizar el código para seguir las mejores prácticas (Separation of Concerns), tal como lo exige el Proyecto 2 a integrar:
*   **Carpetas:** Se crearán `Models/`, `Services/` y `Views/`.
*   **Gestión de Conexión:** Se creará `Services/ConfigDB.cs` para manejar centralizadamente la IP `10.0.0.15` y la nueva base de datos `LoginRolesDB_cif_MINI_ERP`.
*   **MainPage (Login):** Se mantendrá como la puerta de entrada principal y conservará su validación y encriptación SHA2_256, pero su código se limpiará y se adaptará visualmente.
*   **Enrutamiento (AppShell):** Se actualizará para manejar la navegación hacia las vistas correspondientes (`AdminPage`, `SupervisorPage`, `VendedorPage`) organizadas en la carpeta `Views`.

---

## 🗄️ 2. Evolución de la Base de Datos (SQL Server)
La nueva base de datos `LoginRolesDB_cif_MINI_ERP` será la unión de ambos mundos:
1.  **Seguridad (Proyecto Original):** Tablas `Roles` y `Usuarios`.
2.  **Catálogos (Proyecto a Integrar):** Tablas `Categoria`, `Producto`, `Cliente`, `Vendedor`.
3.  **Transaccional (Mejora Solicitada):**
    *   `Venta` (Maestro): Guardará `Id`, `Fecha`, `ClienteId`, `VendedorId` y un cálculo del `TotalGeneral`.
    *   `DetalleVenta` (Detalle): Guardará `Id`, `VentaId`, `ProductoId`, `Cantidad`, `PrecioVentaAplicado`, `SubTotal`, `Itbis`, y `Total`.
    *   *Esto permite que en una sola venta (ticket) un cliente compre N cantidad de productos diferentes.*

---

## 💻 3. Vistas y Lógica de Negocio
*   **VendedorPage (Punto de Venta - POS):** Esta será la pantalla principal para el rol `Vendedor`. Se transformará para permitir seleccionar un cliente, buscar productos, agregarlos a un "carrito" (DetalleVenta) y procesar la factura completa.
*   **AdminPage / RolesPage:** Se mantendrá el CRUD original de seguridad, pero se le agregarán pestañas o navegación hacia el inventario de productos.
*   **ReportesPage (Supervisor):** Se adaptarán las consultas SQL (JOINs) en `VentaService.cs` para soportar la nueva estructura Maestro-Detalle, permitiendo reportes precisos y la generación de Tickets en PDF (QuestPDF) con múltiples líneas de artículos.

---

## 🚀 4. Plan de Ejecución (Paso a Paso)
1.  **Paso 1:** Generar y ejecutar el script SQL actualizado para crear `LoginRolesDB_cif_MINI_ERP` con todas las tablas y relaciones (incluyendo `DetalleVenta`).
2.  **Paso 2:** Crear la estructura de carpetas en el proyecto MAUI (`Models`, `Services`, `Views`).
3.  **Paso 3:** Implementar `ConfigDB.cs` y los modelos (`Usuario.cs`, `Rol.cs`, `Venta.cs`, `DetalleVenta.cs`, etc.).
4.  **Paso 4:** Mover y actualizar `MainPage.xaml` (Login) usando el nuevo `ConfigDB`.
5.  **Paso 5:** Desarrollar `VentaService.cs` para manejar las inserciones complejas (Venta + Múltiples Detalles en una transacción SQL).
6.  **Paso 6:** Transformar `VendedorPage.xaml` en un Punto de Venta (POS) funcional.
7.  **Paso 7:** Ajustar la generación de PDF y los reportes para reflejar múltiples productos.
8.  **Paso 8:** Pruebas finales con los diferentes roles.
