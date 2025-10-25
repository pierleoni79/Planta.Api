// Ruta: /Planta.Contracts/Recibos/ReciboDetailDto.cs | V1.1
using System;

namespace Planta.Contracts.Recibos;

public sealed class ReciboDetailDto
{
    public Guid Id { get; set; }
    public int EmpresaId { get; set; }
    public int Consecutivo { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public ReciboEstado Estado { get; set; }
    public byte DestinoTipo { get; set; }     // <- añade esta si quieres verla en el detalle
    public int VehiculoId { get; set; }
    public int? ConductorId { get; set; }
    public string? Placa { get; set; }
    public int? ClienteId { get; set; }
    public int MaterialId { get; set; }
    public int AlmacenOrigenId { get; set; }
    public decimal Cantidad { get; set; }
    public string? Unidad { get; set; }       // <- añade esta (mapea desde entidad)
    public string? Observaciones { get; set; }
}
