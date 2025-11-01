// Ruta: /Planta.Domain/Common/AggregateRoot.cs | V1.2
#nullable enable
namespace Planta.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected AggregateRoot() : base() { }
    protected AggregateRoot(TId id) : base(id) { }

    protected void Raise(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
