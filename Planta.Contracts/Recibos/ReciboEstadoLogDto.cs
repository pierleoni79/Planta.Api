// Ruta: /Planta.Contracts/Recibos/ReciboEstadoLogDto.cs | V1.1
#nullable enable
using Planta.Contracts.Enums;

namespace Planta.Contracts.Recibos;

public sealed record ReciboEstadoLogDto(
    long Id,
    Guid ReciboId,
    ReciboEstado Estado,
    string? Observacion,
    DateTime FechaCreacionUtc,
    DateTime UltimaActualizacionUtc
);
