// Ruta: /Planta.Contracts/Catalogo/ProductoDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Catalogo;

public sealed record ProductoDto(
    int Id,
    string Nombre,
    string? Codigo,
    int? UnidadId,
    bool Activo
);
