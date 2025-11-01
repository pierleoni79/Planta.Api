// Ruta: /Planta.Domain/Common/IDomainEvent.cs | V1.0
#nullable enable
namespace Planta.Domain.Common;

public interface IDomainEvent { DateTime OccurredOnUtc { get; } }

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
