// Ruta: /Planta.Domain/Transporte/Vehiculo.cs | V1.0
#nullable enable
namespace Planta.Domain.Transporte;

using Planta.Domain.Common;

public sealed class Vehiculo : Entity<int>
{
    public string Placa { get; private set; }
    public int? ClaseVehiculoId { get; private set; }
    public bool Activo { get; private set; }
    public Vehiculo(int id, string placa, int? claseVehiculoId, bool activo) : base(id)
    { Guard.AgainstNullOrWhiteSpace(placa, nameof(placa)); Placa = placa.Trim().ToUpperInvariant(); ClaseVehiculoId = claseVehiculoId; Activo = activo; }
}
