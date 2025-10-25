// Ruta: /Planta.Contracts/Catalogos/ClienteDto.cs | V1.0
namespace Planta.Contracts.Catalogos;

public record ClienteDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = default!;
    public string? NIT { get; init; }
    public bool Activo { get; init; }
    public string? ListaPrecios { get; init; }
}
