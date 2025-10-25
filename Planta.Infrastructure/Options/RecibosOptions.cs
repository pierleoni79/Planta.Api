// Ruta: /Planta.Infrastructure/Options/RecibosOptions.cs | V1.0
namespace Planta.Infrastructure.Options;

public sealed class RecibosOptions
{
    // Si true, asigna ReciboFisicoNumero automáticamente al crear
    public bool AutoGenerarFolioFisico { get; set; } = false;

    // Prefijo del folio, ej. "RF1"
    public string? SerieFolio { get; set; }

    // Reservado para multi-empresa futuro (no se usa hoy)
    public string? ScopeTemplate { get; set; }
}
