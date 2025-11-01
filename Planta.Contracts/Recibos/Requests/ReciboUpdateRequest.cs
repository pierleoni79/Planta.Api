// Ruta: /Planta.Contracts/Recibos/Requests/ReciboUpdateRequest.cs | V1.1
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Planta.Contracts.Recibos.Requests;

/// <summary>Actualiza campos no derivativos (observaciones/documentos).</summary>
public sealed record ReciboUpdateRequest(
    [Required] Guid Id,
    string? Observaciones,
    string? ReciboFisicoNumero,
    string? NumeroGenerado
);
