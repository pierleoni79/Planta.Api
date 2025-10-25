<!-- Ruta: /docs/DOMAIN_MAP.md | V1.0 -->

# Domain Map

## Núcleo
- **Producto** ⟶ pertenece a **Unidad**.
- **Recibo** ⟶ tiene muchos **ReciboDetalle** (producto, cantidad, lote).
- **ProcesoTrituracion** ⟶ referencia a **Recibo** (consumo) y devuelve salidas/mermas.
- **Stock** ⟶ consolidado por `Producto` y `Bodega`.

## Reglas (extracto)
- `Recibo.Estado`: { Borrador, EnTransito, Recibido, Cerrado }.
- Un **Recibo** “Cerrado” no admite más items.
- **Trituración Modo A**: |Entrada − Salida − Residuos| / Entrada ≤ ε (1%), con δ_min=0.1.

## Eventos/Flujos
1. **Recepción**: crear Recibo → agregar detalles → confirmar → afecta `Stock`.
2. **Trituración**: consumir Recibo → calcular balance → registrar residuos → actualizar `Stock`.
3. **Consultas de Stock**: por producto/bodega/fecha; exportables.

## DTOs principales
- `ReciboListItemDto`, `ReciboDetailDto`, `CrearReciboRequest`.
- `CatalogoItemDto` (id, código, nombre).
- `ProcesoResultDto` (balance, residuos, observaciones).
