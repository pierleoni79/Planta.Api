// Ruta: /Planta.Contracts/Catalogo/UnidadDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Catalogo;

public sealed record UnidadDto(
    int Id,
    string Nombre,
    string? Simbolo
);
