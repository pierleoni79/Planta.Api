// Ruta: /Planta.Data/Entities/Proceso.cs | V1.3 (LEGACY — NO EF)
#nullable enable
using System;

namespace Planta.Data.Entities;

/// <summary>
/// LEGACY: se conserva solo como referencia histórica.
/// NO se mapea con EF. El mapeo real de prd.Proceso/prd.ProcesoDet
/// lo hace el dominio: Planta.Domain.Produccion.ProcesoTrituracion*.
/// </summary>
[Obsolete("Legacy. Usa Planta.Domain.Produccion.ProcesoTrituracion*", error: false)]
public sealed class Proceso
{
    public int Id { get; set; }
    public Guid ReciboId { get; set; }     // FK a op.Recibo (configurada en ProcesoTrituracionConfig)
    public int? RecetaId { get; set; }     // nullable según BD
    public byte Estado { get; set; }       // tinyint
    public decimal EntradaM3 { get; set; } // (18,3)
    public string? Observacion { get; set; }
    public DateTimeOffset CreadoEn { get; set; }

    // ⚠️ Sin atributos [Table]/[Column]/[MaxLength] y sin navegaciones,
    // para que EF no lo descubra por convención.
}
