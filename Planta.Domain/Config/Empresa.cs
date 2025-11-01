// Ruta: /Planta.Domain/Config/Empresa.cs | V1.0
#nullable enable
namespace Planta.Domain.Config;

using Planta.Domain.Common;

public sealed class Empresa : Entity<int>
{
    public string Nombre { get; private set; }
    public Empresa(int id, string nombre) : base(id)
    { Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)); Nombre = nombre.Trim(); }
}
