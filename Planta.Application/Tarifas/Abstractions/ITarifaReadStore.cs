// Ruta: /Planta.Application/Tarifas/Abstractions/ITarifaReadStore.cs | V1.1 (fix: alias DestinoTipo)
#nullable enable
using Planta.Contracts.Tarifas;
using DestinoTipoApi = Planta.Contracts.Enums.DestinoTipo;  // ← alias

namespace Planta.Application.Tarifas.Abstractions;

public interface ITarifaReadStore
{
    Task<TarifaVigenteDto> ObtenerTarifaVigenteAsync(
        int claseVehiculoId,
        int materialId,
        DestinoTipoApi destino,        // ← usa el alias (evita ambigüedad)
        int? clienteId,
        int? plantaId,
        CancellationToken ct);
}
