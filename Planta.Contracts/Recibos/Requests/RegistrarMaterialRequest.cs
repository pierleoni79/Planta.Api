// Ruta: /Planta.Contracts/Recibos/Requests/RegistrarMaterialRequest.cs | V1.1
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Planta.Contracts.Recibos.Requests;

public sealed record RegistrarMaterialRequest(
    [Required] Guid Id,
    [Required] int MaterialId,
    [Range(0.0001, double.MaxValue)] decimal Cantidad
);
