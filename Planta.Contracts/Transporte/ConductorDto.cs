// Ruta: /Planta.Contracts/Transporte/ConductorDto.cs | V1.0
namespace Planta.Contracts.Transporte;

public sealed class ConductorDto
{
    public int Id { get; init; }
    public string? Documento { get; init; }
    public string? Nombre { get; init; }
    public bool Activo { get; init; }
}
