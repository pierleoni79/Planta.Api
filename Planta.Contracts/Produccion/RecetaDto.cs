// Ruta: /Planta.Contracts/Produccion/RecetaDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Produccion;

public sealed record RecetaDto(
    int Id,
    string Nombre,
    IReadOnlyList<RecetaDetDto> Detalles
);

public sealed record RecetaDetDto(
    int Id,
    int RecetaId,
    int ProductoId,
    decimal Proporcion
);
