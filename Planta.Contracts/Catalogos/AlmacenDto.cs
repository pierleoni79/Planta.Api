// Ruta: /Planta.Contracts/Catalogos/AlmacenDto.cs | V1.0
namespace Planta.Contracts.Catalogos;

public sealed class AlmacenDto
{
    public int Id { get; init; }
    public int PlantaId { get; init; }
    public string? Nombre { get; init; }
}
