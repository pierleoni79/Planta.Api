// Ruta: /Planta.Contracts/Recibos/Requests/VincularOrigenRequest.cs | V1.1
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Planta.Contracts.Recibos.Requests;

public sealed record VincularOrigenRequest(
    [Required] Guid Id,
    int? PlantaId,
    int? AlmacenOrigenId,
    int? ClienteId
);
