// Ruta: /Planta.Domain/Recibos/Events.cs | V1.0
#nullable enable
namespace Planta.Domain.Recibos;

using Planta.Domain.Common;

public sealed class ReciboCreadoEvent : IDomainEvent
{
    public Recibo Recibo { get; }
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    public ReciboCreadoEvent(Recibo r) => Recibo = r;
}

public sealed class ReciboEstadoCambiadoEvent : IDomainEvent
{
    public Guid ReciboId { get; }
    public int EstadoAnterior { get; }
    public int EstadoNuevo { get; }
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    public ReciboEstadoCambiadoEvent(Guid id, int before, int after)
    { ReciboId = id; EstadoAnterior = before; EstadoNuevo = after; }
}
