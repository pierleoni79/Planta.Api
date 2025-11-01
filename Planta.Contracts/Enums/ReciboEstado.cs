// Ruta: /Planta.Contracts/Enums/ReciboEstado.cs | V1.1
#nullable enable
namespace Planta.Contracts.Enums;

/// <summary>Estados oficiales alineados a BD (10/12/20/30/90/99).</summary>
public enum ReciboEstado
{
    EnTransitoPlanta = 10,
    Descargando = 12,
    Procesado = 20,
    Cerrado = 30,
    Anulado = 90,
    Error = 99
}
