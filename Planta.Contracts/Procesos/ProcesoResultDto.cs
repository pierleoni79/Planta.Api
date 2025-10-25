// Ruta: /Planta.Contracts/Procesos/ProcesoResultDto.cs | V1.0
namespace Planta.Contracts.Procesos;

public sealed class ProcesoResultDto
{
    public double BalancePorc { get; set; }       // 0..100 (3 decimales)
    public bool Cumple { get; set; }       // true si ≤ 1%
    public double Epsilon { get; set; }       // 0.01
    public double DeltaMin { get; set; }       // 0.1
    public string? Observaciones { get; set; }
}
