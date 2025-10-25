<!-- Ruta: /docs/CACHING_ETAG.md | V1.0 -->

# Caching con ETag (Módulo B)

## Objetivo
Reducir ancho de banda y latencia en listados de catálogos, garantizando coherencia entre cliente y servidor.

## Reglas
- Generar ETag **débil** (`W/`) a partir del **contenido serializado** (ej: hash SHA256 del payload JSON ordenado).
- Enviar siempre:
  - `ETag: W/"<hash>"`
  - `Cache-Control: public,max-age=60`
- Soportar condición:
  - Si request trae `If-None-Match` y coincide → **304 Not Modified** (sin body).

## Ciclo cliente
1. `GET /api/catalogos/materiales` → recibe `ETag`.
2. Guarda ETag localmente (por endpoint y parámetros).
3. Siguientes GET envían `If-None-Match: <ETag guardado>`.
4. Si 304 → usar cache local; si 200 → actualizar cache y ETag.

## Ejemplo (curl)
```bash
# 1ª vez
curl -i https://localhost:7261/api/catalogos/materiales

# Revalidación
curl -i -H 'If-None-Match: W/"b2f-3a4f..."' \
     https://localhost:7261/api/catalogos/materiales
