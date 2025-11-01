// Ruta: /Planta.Contracts/Recibos/Requests/ReciboCreateFullRequest.cs | V1.0 (opcional Fase 2)
#nullable enable
using System.ComponentModel.DataAnnotations;
using Planta.Contracts.Enums;

namespace Planta.Contracts.Recibos.Requests;

/// <summary>
/// Request para crear el Recibo en una sola llamada (Fase 2).
/// </summary>
public sealed record ReciboCreateFullRequest(
    [Required] int EmpresaId,
    [Required] int VehiculoId,
    [Required] string PlacaSnapshot,
    int? ConductorId,
    [Required] string ConductorNombreSnapshot,
    [Required] int MaterialId,
    [Required] DestinoTipo DestinoTipo,   // Planta | ClienteDirecto
    int? ClienteId,                       // requerido si ClienteDirecto
    int? PlantaDestinoId,                 // requerido si Planta
    [Range(0.0001, double.MaxValue)] decimal Cantidad,
    string? UnidadAplicada,               // "m3" | "viaje" (recomendado persistir en Fase 2)
    [Required] int AlmacenOrigenId,
    string? Observaciones
);
