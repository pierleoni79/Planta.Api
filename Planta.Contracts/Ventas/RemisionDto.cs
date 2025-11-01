// Ruta: /Planta.Contracts/Ventas/RemisionDto.cs | V1.1
#nullable enable
namespace Planta.Contracts.Ventas;

public sealed record RemisionDto(
    int Id,
    int EmpresaId,
    int ClienteId,
    DateTime FechaUtc,
    IReadOnlyList<RemisionDetDto> Detalles
);

public sealed record RemisionDetDto(
    int Id,
    int RemisionId,
    int ProductoId,
    decimal Cantidad
);
