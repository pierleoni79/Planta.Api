using System.ComponentModel.DataAnnotations;

namespace Planta.Contracts.Recibos;

public sealed class DescargaInicioRequest
{
    [Required, StringLength(128)]
    public string Usuario { get; set; } = default!;

    [StringLength(1024)]
    public string? Observaciones { get; set; }

    /// <summary>
    /// Clave idempotente (solo el key). Sugerencia: "&lt;guid&gt;".
    /// Nota: el servidor compone el scope internamente (p.ej. "descarga-inicio:&lt;key&gt;").
    /// </summary>
    [Required, StringLength(128)]
    public string IdempotencyKey { get; set; } = default!;
}
