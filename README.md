# Librería October | Proyecto Socioformativo — Programación Web II

## Estado confirmado: 28 de octubre de 2026

**Arquitectura:** `Blazor.Client` (WebAssembly) + `Blazor.Server` (API REST) + `Blazor.Shared` (DTO y categorías).
**Persistencia utilizada en el equipo:** SQL Server, base `LibreriaOctober`, Entity Framework Core 8.
**Migración:** `20261028200328_InitialCreate` aplicada correctamente mediante `dotnet ef database update`.
**Evidencia:** registro de terminal con `CREATE DATABASE`, seis tablas de negocio, `__EFMigrationsHistory` y resultado `Done.`

> **Atención a versiones:** el ZIP inicial distribuido contenía `EnsureCreated()` y SQLite como predeterminado. En el Visual Studio del equipo se cambió `DatabaseProvider` a `SqlServer`, se reemplazó `EnsureCreated()` por `Migrate()` y se creó una migración. Antes de entregar el software comprimido, generar un ZIP **desde la carpeta local actualizada** e incluir `Migrations/`. No se debe sustituirlo por el ZIP inicial sin esos cambios.

## Cómo iniciar en Visual Studio

1. Abrir `proyectoLSP.sln` en Visual Studio 2022 con .NET 8.
2. Confirmar que SQL Server está iniciado y que la base `LibreriaOctober` existe.
3. Verificar la configuración local en `Blazor.Server/appsettings.json`:

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "LibreriaSqlite": "Data Source=libreria.db",
    "LibreriaConnection": "Server=localhost;Database=LibreriaOctober;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "SeedDemo": true
}
```

4. Revisar `Program.cs`: `db.Database.Migrate();` en lugar de `EnsureCreated()` **cuando se utilizan migraciones**.
5. Configurar inicio múltiple: `Blazor.Server` y `Blazor.Client`.
6. Ejecutar con F5; cliente `http://localhost:5236`, API `http://localhost:5274`, Swagger `http://localhost:5274/swagger` y salud `http://localhost:5274/api/health` (si se mantienen los perfiles originales).

## Funciones y reglas de negocio

| Módulo | Funcionalidad | Impacto |
|---|---|---|
| Libros | Crear, consultar, editar, desactivar | Stock inicial en alta |
| Categorías | Catálogo fijo: Literatura, Académico, Infantil, Historia, Ciencia, Religión, Arte, Otros | Filtro |
| Compras | Registrar varios libros, proveedor opcional | Suma stock |
| Ventas | Registrar varios libros; nombre/NIT opcionales | Descuenta stock |
| Venta general | Sin nombre ni NIT/CI | Se muestra como “Venta general” |
| Inventario | Consultar, filtrar, ver movimientos | Solo lectura |
| Dashboard | Indicadores y alertas | Solo lectura |

## Base de datos SQL Server

Tablas funcionales: `Libros`, `Compras`, `DetallesCompra`, `Ventas`, `DetallesVenta`, `MovimientosStock`; tabla técnica: `__EFMigrationsHistory`.

```powershell
dotnet ef migrations list
dotnet ef database update
```

La primera migración **ya fue aplicada** en el equipo; no hace falta recrearla. Para generar cambios posteriores, modificar el modelo, crear una nueva migración y probarla. **No eliminar la base ni ejecutar EnsureCreated junto con migraciones**.

**Observación técnica:** el esquema aplicado incluye columnas redundantes `IdCompra`/`CompraIdCompra` y `IdVenta`/`VentaIdVenta` en las tablas de detalle. Requieren revisión del mapeo de claves foráneas antes de un uso productivo.

## Evidencias para la defensa

- Libro nuevo con 10 unidades; compra de 5: stock 15; venta general de 3: stock 12.
- Venta de 50 con 12 disponibles: rechazo sin afectar saldo.
- Filtro por categoría e historial INICIAL, COMPRA, VENTA.
- SSMS con base y tablas; registro `InitialCreate` en `__EFMigrationsHistory`.
- Cinco capturas de las pantallas reales y demostración en Swagger.

## Guía socioformativa: revisión de alcance

La guía exige CRUD de todas las tablas y un CRUD/documentación individual por integrante. **Actualmente libros tiene CRUD (baja lógica), pero compras y ventas exponen GET/POST y movimientos son de consulta.** Se debe completar o declarar esta limitación. No consta en los archivos una asignación real de tareas por integrantes ni investigación práctica con resultados de campo. El informe Word incluye el plan y espacios para evidencias, no resultados inventados.