# Plantillas de encabezados obligatorios

**Ruta:** /docs/9-templates/ENCABEZADOS.md | **Versión:** V1.0  
**Módulo:** 9-templates — encabezados  
**Objetivo:** Centralizar las plantillas de cabecera obligatoria para todos los archivos del proyecto.  
**Impacto:** Aplica a código (.cs), SQL (.sql) y documentación (.md).  
**Requisitos:** Incluir la cabecera al inicio de cada archivo nuevo o modificado.  
**Notas:** El hook de pre-commit puede validar estas cabeceras automáticamente si está habilitado.

---

## 1) Estructura estándar de cabecera

- **Siempre al inicio** del archivo.
- Debe incluir: **Ruta**, **Versión (Vx.y)**, **Módulo**, **Objetivo**, **Impacto**, **Requisitos**, **Notas**.

---

## 2) Plantillas listas para copiar

### 2.1 C# (.cs)
```csharp
// Ruta: /<carpeta/archivo.ext> | V1.0
// Módulo: <A/B/C/D/...> — <nombre>
// Objetivo: <1 línea>
// Impacto: <capas afectadas / entidades / endpoints>
// Requisitos: <prerrequisitos>
// Notas: <detalle corto si aplica>
