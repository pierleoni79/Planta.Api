#nullable enable
namespace Planta.Contracts.Recibos;

public sealed class CrearReciboResponse
{
    public Guid ReciboId { get; init; }
    public ReciboEstado Estado { get; init; }
    public string ETag { get; init; } = string.Empty;
}
