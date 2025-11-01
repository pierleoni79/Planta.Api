// Ruta: /Planta.Application/Tarifas/Queries/ObtenerTarifaVigente/ObtenerTarifaVigenteHandler.cs | V1.1 (fix: no depender de Data)
#nullable enable
using MediatR;
using Planta.Contracts.Tarifas;
using Planta.Application.Tarifas.Abstractions; // ← usar la abstracción de Application

namespace Planta.Application.Tarifas.Queries.ObtenerTarifaVigente;

public sealed class ObtenerTarifaVigenteHandler(ITarifaReadStore rs)
    : IRequestHandler<ObtenerTarifaVigenteQuery, TarifaVigenteDto>
{
    public Task<TarifaVigenteDto> Handle(ObtenerTarifaVigenteQuery request, CancellationToken ct)
        => rs.ObtenerTarifaVigenteAsync(
            request.ClaseVehiculoId,
            request.MaterialId,
            request.Destino,
            request.ClienteId,
            request.PlantaId,
            ct);
}
