// Ruta: /Planta.Domain/Config/Planta.cs | V1.0
#nullable enable
namespace Planta.Domain.Config;

using Planta.Domain.Common;

public sealed class PlantaSite : Entity<int>
{
    public int EmpresaId { get; private set; }
    public string Nombre { get; private set; }
    public PlantaSite(int id, int empresaId, string nombre) : base(id)
    { EmpresaId = empresaId; Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)); Nombre = nombre.Trim(); }
}
