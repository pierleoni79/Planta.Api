#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Planta.Contracts.Common;
using Planta.Contracts.Recibos;
using Planta.Contracts.Procesos;   // DescargaFin: payload de proceso

namespace Planta.Application.Abstractions;

/// <summary>
/// Servicio de aplicación para Recibos con ETag e Idempotencia (scope:key).
/// Mantener esta interfaz 1:1 con la implementación en Infrastructure.
/// </summary>
public interface IRecibosService
{
    /// <summary>Listado paginado con filtros opcionales.</summary>
    Task<PagedResult<ReciboListItemDto>> ListarAsync(
        PagedRequest req,
        int? empresaId, ReciboEstado? estado, int? clienteId,
        DateTimeOffset? desde, DateTimeOffset? hasta,
        string? search, CancellationToken ct);

    /// <summary>Obtiene un recibo por Id (sin validación condicional).</summary>
    Task<ReciboDetailDto?> ObtenerAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// GET condicional por ETag. Si <paramref name="ifNoneMatch"/> coincide con el ETag actual,
    /// retorna <c>NotModified = true</c> y el ETag, con <c>Dto = null</c>.
    /// </summary>
    Task<(ReciboDetailDto? Dto, string? ETag, bool NotModified)>
        ObtenerConEtagAsync(Guid id, string? ifNoneMatch, CancellationToken ct);

    /// <summary>
    /// Crea un recibo; aplica idempotencia por (scope:key). La <paramref name="idempotencyKeyHeader"/>
    /// puede venir de cabecera externa. Devuelve si el resultado fue idempotente.
    /// </summary>
    Task<(CrearReciboResponse Response, bool Idempotent)> CrearAsync(
        CrearReciboRequest req,
        string? idempotencyKeyHeader,
        CancellationToken ct);

    /// <summary>
    /// Cambio de estado genérico; devuelve DTO y nuevo ETag. Idempotencia opcional por key.
    /// </summary>
    Task<(ReciboDetailDto Dto, string ETag)> CambiarEstadoAsync(
        Guid id,
        ReciboEstado nuevo,
        string? comentario,
        string? idempotencyKey,
        CancellationToken ct);

    /// <summary>
    /// Check-in en planta. Orden: idempotencia (scope:key) → If-Match (ETag) →
    /// reglas de transición → persistir → devolver ETag nuevo e indicador de idempotencia.
    /// </summary>
    Task<(ReciboDetailDto Dto, string ETag, bool Idempotent)> CheckinAsync(
        Guid id,
        string? gps,
        string? comentario,
        string? idempotencyKey,
        string ifMatch,
        string etag,
        CancellationToken ct);

    /// <summary>
    /// Marca el inicio de la descarga (no cambia estado si tu regla así lo define).
    /// Aplica If-Match (ETag) + Idempotency-Key; devuelve DTO y nuevo ETag.
    /// </summary>
    Task<(ReciboDetailDto Dto, string ETag)> DescargaInicioAsync(
        Guid id,
        string? comentario,
        string ifMatch,
        string etag,
        string? idempotencyKey,
        CancellationToken ct);

    /// <summary>
    /// Finaliza la descarga creando el Proceso (control de masa).
    /// Aplica If-Match (ETag) + Idempotency-Key; devuelve DTO y nuevo ETag.
    /// </summary>
    Task<(ReciboDetailDto Dto, string ETag)> DescargaFinAsync(
        Guid id,
        ProcesarTrituracionRequest proceso,
        string? comentario,
        string ifMatch,
        string etag,
        string? idempotencyKey,
        CancellationToken ct);
}
