// Ruta: /Planta.Domain/Common/Entity.cs | V1.1
#nullable enable
namespace Planta.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }

    // Necesario para serialización/EF y para evitar errores de compilación por falta de ctor vacío.
    protected Entity() => Id = default!;

    // Ctor conveniente cuando creas la entidad con su identidad explícita.
    protected Entity(TId id) => Id = id;
}
