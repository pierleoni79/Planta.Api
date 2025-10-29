// Ruta: /Planta.Contracts/Recibos/ReciboListItemDto.cs | V1.1
#nullable enable
using System;

namespace Planta.Contracts.Recibos;

public sealed class ReciboListItemDto
{
    public Guid Id { get; set; }
    public int EmpresaId { get; set; }
    public int Consecutivo { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public ReciboEstado Estado { get; set; }
    public int VehiculoId { get; set; }

    public string? Placa { get; set; }        // <- de PlacaSnapshot
    public string? Conductor { get; set; }    // <- de ConductorNombreSnapshot  (NUEVO)

    public int? ClienteId { get; set; }
    public decimal Cantidad { get; set; }
}
