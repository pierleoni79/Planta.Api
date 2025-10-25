// Ruta: /Planta.Contracts/Recibos/CrearReciboRequest.cs | V1.2
using System.ComponentModel.DataAnnotations;

namespace Planta.Contracts.Recibos;

public sealed class CrearReciboRequest
{
    [Required] public byte DestinoTipo { get; set; }  // 1=Planta, 2=ClienteDirecto
    [Required] public int EmpresaId { get; set; }
    public int? ClienteId { get; set; }               // obligatorio si DestinoTipo=2
    [Required] public int VehiculoId { get; set; }
    public int? ConductorId { get; set; }
    [Required] public int AlmacenOrigenId { get; set; }
    [Required] public int MaterialId { get; set; }
    [Required] public decimal Cantidad { get; set; }  // m3
    [Required] public string? Unidad { get; set; }    // <- añade esta
    public string? Observaciones { get; set; }
    public string? IdempotencyKey { get; set; }
}
