// Ruta: /Planta.Domain/Common/Entity.cs | V1.1
#nullable enable
namespace Planta.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }

    // Necesario para serializaci�n/EF y para evitar errores de compilaci�n por falta de ctor vac�o.
    protected Entity() => Id = default!;

    // Ctor conveniente cuando creas la entidad con su identidad expl�cita.
    protected Entity(TId id) => Id = id;
}
