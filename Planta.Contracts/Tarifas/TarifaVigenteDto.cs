// Ruta: /Planta.Contracts/Tarifas/TarifaVigenteDto.cs | V1.0
#nullable enable
namespace Planta.Contracts.Tarifas;

/// <summary>Tarifa vigente encontrada (o nula si no hay match).</summary>
public sealed record TarifaVigenteDto(
    int? TarifaId,
    string? Unidad,    // "m3" | "viaje" | null si no hay tarifa
    decimal Precio,
    int Prioridad
);
