// Ruta: /Planta.Contracts/Recibos/Requests/VincularTransporteRequest.cs | V1.1
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Planta.Contracts.Recibos.Requests;

public sealed record VincularTransporteRequest(
    [Required] Guid Id,
    int? VehiculoId,
    int? ConductorId,
    string? PlacaSnapshot = null,
    string? ConductorNombreSnapshot = null
);
