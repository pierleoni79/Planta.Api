// Ruta: /Planta.Domain/CRM/Cliente.cs | V1.0
#nullable enable
namespace Planta.Domain.CRM;

using Planta.Domain.Common;

public sealed class Cliente : Entity<int>
{
    public string Nombre { get; private set; }
    public bool Activo { get; private set; }
    public Cliente(int id, string nombre, bool activo) : base(id)
    { Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)); Nombre = nombre.Trim(); Activo = activo; }
}
