// Ruta: /Planta.Domain/Transporte/Conductor.cs | V1.0
#nullable enable
namespace Planta.Domain.Transporte;

using Planta.Domain.Common;

public sealed class Conductor : Entity<int>
{
    public string Nombre { get; private set; }
    public bool Activo { get; private set; }
    public Conductor(int id, string nombre, bool activo) : base(id)
    { Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)); Nombre = nombre.Trim(); Activo = activo; }
}
