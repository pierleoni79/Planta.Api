// Ruta: /Planta.Domain/Produccion/ProcesoTrituracion.cs | V1.0
namespace Planta.Domain.Produccion;

public sealed class ProcesoTrituracion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReciboId { get; set; }
    public double PesoEntrada { get; set; }
    public double PesoSalida { get; set; }
    public double Residuos { get; set; }
    public double BalancePorc { get; set; }  // 0..100
    public bool Cumple { get; set; }
    public double Epsilon { get; set; } = 0.01;
    public double DeltaMin { get; set; } = 0.1;
    public string? Observaciones { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ProcesoTrituracionSalida> Salidas { get; set; } = new List<ProcesoTrituracionSalida>();
}
