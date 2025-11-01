// Ruta: /Planta.Domain/Config/Almacen.cs | V1.0
#nullable enable
namespace Planta.Domain.Config;

using Planta.Domain.Common;

public sealed class Almacen : Entity<int>
{
    public int EmpresaId { get; private set; }
    public int? PlantaId { get; private set; }
    public string Nombre { get; private set; }

    public Almacen(int id, int empresaId, string nombre, int? plantaId = null) : base(id)
    { EmpresaId = empresaId; PlantaId = plantaId; Guard.AgainstNullOrWhiteSpace(nombre, nameof(nombre)); Nombre = nombre.Trim(); }
}
