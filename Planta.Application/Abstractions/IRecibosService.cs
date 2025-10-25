// Ruta: /Planta.Application/Abstractions/IRecibosService.cs | V1.2
using System;
using System.Threading;
using System.Threading.Tasks;
using Planta.Contracts.Common;
using Planta.Contracts.Recibos;

namespace Planta.Application.Abstractions;

public interface IRecibosService
{
    Task<PagedResult<ReciboListItemDto>> ListarAsync(
        PagedRequest req,
        int? empresaId, int? estado, int? clienteId,
        DateTimeOffset? desde, DateTimeOffset? hasta,
        string? search, CancellationToken ct);

    Task<ReciboDetailDto?> ObtenerAsync(Guid id, CancellationToken ct);

    /// <summary>Crea un recibo; si llega Idempotency-Key (header o body) y existe, devuelve el existente.</summary>
    Task<ReciboDetailDto> CrearAsync(CrearReciboRequest req, string? idempotencyKeyHeader, string? usuario, CancellationToken ct);

    Task<ReciboDetailDto> CambiarEstadoAsync(Guid id, ReciboEstado nuevo, string? comentario, CancellationToken ct);

    /// <summary>Marca check-in en planta: EnTransito_Planta → EnPatioPlanta y registra GPS/nota en op.ReciboEstadoLog.</summary>
    Task<ReciboDetailDto> CheckinAsync(Guid id, string? gps, string? comentario, CancellationToken ct);
}
