// Ruta: /Planta.Contracts/Recibos/Requests/CambiarEstadoRequest.cs | V1.1
#nullable enable
using System.ComponentModel.DataAnnotations;
using Planta.Contracts.Enums;

namespace Planta.Contracts.Recibos.Requests;

public sealed record CambiarEstadoRequest(
    [Required] Guid Id,
    [Required] ReciboEstado NuevoEstado,
    string? Observacion = null,
    string? IfMatchETag = null  // opcional: control de concurrencia optimista
);
