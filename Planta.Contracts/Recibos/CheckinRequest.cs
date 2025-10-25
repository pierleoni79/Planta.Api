// Ruta: /Planta.Contracts/Recibos/CheckinRequest.cs | V1.2
#nullable enable
namespace Planta.Contracts.Recibos;

public sealed class CheckinRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}
