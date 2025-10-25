# Planta — Ecosistema de Inventario y Operaciones

Solución multi-módulo (.NET) para recepción de material, procesos de planta, catálogos, stock y trazabilidad. Incluye API, lógica de aplicación, dominios, reportes y cliente móvil (MAUI).

## Requisitos
- .NET SDK 8.0.x
- SQL Server (local o remoto)
- (Opcional) Visual Studio 2022 / VS Code

## Estructura (alto nivel)
- `Planta.Api` — API ASP.NET Core (Swagger, endpoints de negocio)
- `Planta.Application` — Casos de uso (MediatR, validaciones)
- `Planta.Domain` — Entidades y reglas de dominio
- `Planta.Infrastructure` — Persistencia / servicios externos
- `Planta.Contracts` — DTOs y contratos
- `Planta.Data` — Inicialización/seed/scripts utilitarios
- `Planta.Mobile` — Cliente MAUI
- `Planta.Reportes.*` — Generación de reportes

## Compilar y ejecutar (local)
```bash
dotnet --version
dotnet restore ./Planta.Api.sln
dotnet build   ./Planta.Api.sln -c Release
dotnet run --project ./Planta.Api/Planta.Api.csproj
