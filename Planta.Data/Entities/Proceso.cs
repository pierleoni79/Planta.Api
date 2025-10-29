// Ruta: /Planta.Data/Entities/Proceso.cs | V1.2
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planta.Data.Entities;

[Table("Proceso", Schema = "prd")]
public sealed class Proceso
{
    public int Id { get; set; }

    // FK al recibo
    public Guid ReciboId { get; set; }

    // Según el inventario de columnas, RecetaId puede ser NULL
    public int? RecetaId { get; set; }

    // Manejo futuro de estados del proceso (tinyint)
    public byte Estado { get; set; }

    // decimal(18,3) en BD
    [Column(TypeName = "decimal(18,3)")]
    public decimal EntradaM3 { get; set; }

    [MaxLength(1024)]
    public string? Observacion { get; set; }

    // DEFAULT sysdatetimeoffset() en BD (puedes reafirmarlo en el Configuration)
    public DateTimeOffset CreadoEn { get; set; }

    // ---- Navegaciones ----
    public Recibo? Recibo { get; set; }
    public ICollection<ProcesoDet> Detalles { get; set; } = new List<ProcesoDet>();
}
