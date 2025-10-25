// Ruta: /Planta.Contracts/Recibos/ReciboEstado.cs | V1.1
namespace Planta.Contracts.Recibos;

public enum ReciboEstado
{
    Borrador = 0,
    EnTransito_Planta = 10,
    EnTransito_Cliente = 12,
    EnPatioPlanta = 20,
    Descargando = 30,
    Procesado = 40,
    Cerrado = 90,
    Anulado = 99
}
