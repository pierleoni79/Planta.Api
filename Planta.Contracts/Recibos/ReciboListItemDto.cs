// Ruta: /Planta.Contracts/Recibos/ReciboListItemDto.cs | V1.1
#nullable enable
using Planta.Contracts.Enums;

namespace Planta.Contracts.Recibos;

/// <summary>Item liviano para listados/paginación.</summary>
public sealed record ReciboListItemDto(
    Guid Id,
    int EmpresaId,
    ReciboEstado Estado,
    DestinoTipo DestinoTipo,
    decimal Cantidad,
    string? PlacaSnapshot,
    string? ConductorNombreSnapshot,
    string? NumeroGenerado,
    DateTime FechaCreacionUtc,
    string ETag
);
