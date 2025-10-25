// Ruta: /Planta.Contracts/Catalogos/ProductoDto.cs | V1.0
namespace Planta.Contracts.Catalogos;

public sealed class ProductoDto
{
    public int Id { get; init; }
    public string? Codigo { get; init; }
    public string? Nombre { get; init; }
    public string? Unidad { get; init; }
    public bool VendibleEnCantera { get; init; }
}
