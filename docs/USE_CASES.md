<!-- Ruta: /docs/USE_CASES.md | V1.0 -->

# Use Cases

## UC-B1 Listar Catálogos (cacheable)
**Actor:** Web/Móvil  
**Flujo:** GET materiales/unidades → guarda ETag → revalida con If-None-Match.  
**Éxito:** 200 con datos o 304.

## UC-C1 Crear Recibo
**Actor:** Operador  
**Flujo:** POST crear → POST items → confirmar.  
**Reglas:** Validar productos/unidades, estados permitidos.

## UC-C2 Listar Recibos (paginado)
**Actor:** Admin/Operador  
**Flujo:** GET con filtros → `PagedResult`.

## UC-D1 Procesar Trituración (Modo A)
**Actor:** Responsable de planta  
**Flujo:** POST procesar → verifica balance (ε=1%, δ_min=0.1) → registra salida/merma.  
**Errores:** 422 si no cumple balance.

## UC-E1 Consultar Stock y Trazabilidad (pendiente)
**Actor:** Control inventario  
**Flujo:** Filtros por producto/bodega/fecha → export.
