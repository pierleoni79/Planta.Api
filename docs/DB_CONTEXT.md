
---

# Paso 3 — Crear `docs/DB_CONTEXT.md`
**Ruta:** `docs/DB_CONTEXT.md`  
**Contenido:**
```md
<!-- Ruta: /docs/DB_CONTEXT.md | V1.0 -->

# DB Context & Esquema

## Baseline
- Se parte de la BD existente (script auditado). No se modifican tablas base salvo parches de integridad/índices.

## Conexión
- `Planta.Api` lee cadena de conexión desde `appsettings.{Environment}.json`.
- Clave sugerida: `ConnectionStrings:PlantaDb`.

## Convenciones
- PKs: `Id` (int para `Producto`, `Guid` para `Recibo`).
- FKs explícitas y `ON DELETE` conservadoras.
- Índices:
  - Búsquedas comunes (ej. `IX_Recibo_EmpresaId_Consecutivo`).
  - Catálogos por `Nombre`/`Codigo`.
- Auditoría mínima: `CreatedAt`, `UpdatedAt`, `User`.

## Performance
- Índices cubiertos para listados con filtros.
- Paginación server-side (OFFSET/FETCH).
- Evitar N+1 con `Include` o proyecciones.

## Migraciones
- Mantener scripts versionados en `Planta.Data` (T-SQL).
- Documentar parches aquí con fecha y propósito.

## Entidades clave (resumen)
- **Producto**(Id:int, Nombre, UnidadId, …)
- **Unidad**(Id:int, Codigo, Nombre)
- **Recibo**(Id:guid, EmpresaId, Consecutivo, Fecha, Estado, …)
- **ReciboDetalle**(Id, ReciboId, ProductoId, Cantidad, Lote, …)
- **ProcesoTrituracion**(Id, ReciboId, PesoEntrada, PesoSalida, Merma, …)
- **Stock**(ProductoId, Existencia, Bodega, …)
