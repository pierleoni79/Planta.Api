// Ruta: /Planta.Contracts/Recibos/DescargaFinRequest.cs | V1.2
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Planta.Contracts.Recibos;

public sealed class DescargaFinRequest
{
    [Required, StringLength(128)]
    public string Usuario { get; set; } = default!;

    [StringLength(1024)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Clave idempotente (solo el key). Sugerencia: "&lt;guid&gt;".
    /// Nota: el servidor compone internamente el scope "descarga-fin:&lt;key&gt;".
    /// </summary>
    [Required, StringLength(128)]
    public string IdempotencyKey { get; set; } = default!;

    [Required]
    public ProcesoFinDto Proceso { get; set; } = default!;
}

public sealed class ProcesoFinDto
{
    [Range(1, int.MaxValue)]
    public int RecetaId { get; set; }

    [Required]
    public MedidaDto Entrada { get; set; } = default!;

    [Required]
    public List<SalidaDto> Salidas { get; set; } = new();

    /// <summary>% tolerancia (ej.: 1.0 = ±1%). Si null, usar default por planta/receta.</summary>
    public decimal? ToleranciaPct { get; set; }

    /// <summary>Observación específica del proceso (opcional).</summary>
    [StringLength(1024)]
    public string? Observaciones { get; set; }
}

public sealed class MedidaDto
{
    // > 0 (no permitir 0)
    [Range(typeof(decimal), "0.0000001", "79228162514264337593543950335")]
    public decimal Cantidad { get; set; }

    /// <summary>Usar "m3". Si la app trabaja en otra unidad, convertir antes de enviar.</summary>
    [Required, StringLength(20)]
    public string Unidad { get; set; } = "m3";
}

public sealed class SalidaDto
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    // > 0 (no permitir 0)
    [Range(typeof(decimal), "0.0000001", "79228162514264337593543950335")]
    public decimal Cantidad { get; set; }

    /// <summary>Usar "m3".</summary>
    [Required, StringLength(20)]
    public string Unidad { get; set; } = "m3";
}
