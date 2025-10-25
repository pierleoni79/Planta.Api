// Ruta: /Planta.Contracts/Procesos/ProcesarTrituracionRequest.cs | V1.0
namespace Planta.Contracts.Procesos;

public sealed class ProcesarTrituracionRequest
{
    public double PesoEntrada { get; set; }       // obligatorio
    public double PesoSalida { get; set; }       // obligatorio
    public double Residuos { get; set; }       // opcional (0 si no aplica)
    public string? Observaciones { get; set; }
}
