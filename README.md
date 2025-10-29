# Planta.Api — Vista general del repo

Este README está optimizado para auditorías rápidas (por humanos y por ChatGPT) y **muestra automáticamente el árbol completo de la rama `master`** entre las marcas de abajo.

---

## Navegación rápida (carpetas clave)

- **API** → [`/Planta.Api`](Planta.Api/)
  - `Program.cs`, `appsettings.*.json`
- **Application** → [`/Planta.Application`](Planta.Application/)
  - `Features/**` (CQRS, handlers, validaciones)
- **Contracts** → [`/Planta.Contracts`](Planta.Contracts/)
  - DTOs públicos
- **Domain** → [`/Planta.Domain`](Planta.Domain/)
  - `Entities/**`, `ValueObjects/**`, `Enums/**`
- **Data (EF Core)** → [`/Planta.Data`](Planta.Data/)
  - `PlantaDbContext.cs`, `Configurations/**`, `Migrations/**`
- **Infrastructure** → [`/Planta.Infrastructure`](Planta.Infrastructure/)
  - Integraciones externas/adapters
- **Mobile (MAUI)** → [`/Planta.Mobile`](Planta.Mobile/)
- **Reportes** → [`/Planta.Reportes.*`](./)
- **Tests** → [`/tests`](tests/)
- **Docs** → [`/docs`](docs/)

> Si alguna ruta difiere, actualízala aquí para mantener la navegación consistente.

---

## Cómo pedirme auditorías (desde ChatGPT)

> **AUDITA REPO (profunda)** — repo: `pierleoni79/Planta.Api` — rama: `master` — **sin leer README**

Opcional (recomendado): usa tag/commit para snapshot inmutable, por ejemplo: `audit-YYYYMMDD-HHMM`.

---

## Estructura de la rama `master` (auto-generada)

> _Se rellena automáticamente por GitHub Actions en cada push a `master`._

<!-- BEGIN TREE -->
_(pendiente de primera ejecución del workflow)_
<!-- END TREE -->

---

## Comandos útiles

```bash
dotnet --info
dotnet restore
dotnet build -c Release
dotnet test
