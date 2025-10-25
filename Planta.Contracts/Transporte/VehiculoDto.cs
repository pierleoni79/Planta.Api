// Ruta: /Planta.Contracts/Transporte/VehiculoDto.cs | V1.0
namespace Planta.Contracts.Transporte;

public sealed class VehiculoDto
{
    public int Id { get; init; }
    public string? Placa { get; init; }
    public string? Clase { get; init; }
    public bool Activo { get; init; }
}
