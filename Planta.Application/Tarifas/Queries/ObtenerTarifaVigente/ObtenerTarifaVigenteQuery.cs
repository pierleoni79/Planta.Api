// Ruta: /Planta.Application/Tarifas/Queries/ObtenerTarifaVigente/ObtenerTarifaVigenteQuery.cs | V1.1
#nullable enable
using MediatR;
using Planta.Contracts.Tarifas;
using DestinoTipoApi = Planta.Contracts.Enums.DestinoTipo;

namespace Planta.Application.Tarifas.Queries.ObtenerTarifaVigente;

public sealed record ObtenerTarifaVigenteQuery(
    int ClaseVehiculoId,
    int MaterialId,
    DestinoTipoApi Destino,   // ← alias
    int? ClienteId,
    int? PlantaId
) : IRequest<TarifaVigenteDto>;
