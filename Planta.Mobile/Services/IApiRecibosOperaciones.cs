//Ruta: /Planta.Mobile/Services/IApiRecibosOperaciones.cs | V1.0
#nullable enable
namespace Planta.Mobile.Services;

public interface IApiRecibosOperaciones
{
    Task EjecutarCheckInAsync(Guid id, string currentEtag, Guid idempotencyKey, CancellationToken ct);
    Task EjecutarDescargaInicioAsync(Guid id, string currentEtag, Guid idempotencyKey, CancellationToken ct);
    Task EjecutarDescargaFinAsync(Guid id, string currentEtag, Guid idempotencyKey, CancellationToken ct);
}