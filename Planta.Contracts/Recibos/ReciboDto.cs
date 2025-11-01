// Ruta: /Planta.Contracts/Recibos/ReciboDto.cs | V1.1
#nullable enable
using Planta.Contracts.Enums;

namespace Planta.Contracts.Recibos;

/// <summary>DTO completo de op.Recibo, incluye ETag para cacheo condicional.</summary>
public sealed record ReciboDto(
    Guid Id,
    int EmpresaId,
    int? PlantaId,
    int? AlmacenOrigenId,
    int? ClienteId,
    int? VehiculoId,
    int? ConductorId,
    int? MaterialId,
    ReciboEstado Estado,
    DestinoTipo DestinoTipo,
    decimal Cantidad,
    string? Observaciones,
    string? PlacaSnapshot,
    string? ConductorNombreSnapshot,
    string? ReciboFisicoNumero,
    string? NumeroGenerado,
    string? IdempotencyKey,
    DateTime FechaCreacionUtc,
    DateTime UltimaActualizacionUtc,
    string ETag
);
