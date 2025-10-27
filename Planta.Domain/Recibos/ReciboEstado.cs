//Planta.Domain/Recibos/ReciboEstado.cs
namespace Planta.Domain.Recibos;

public enum ReciboEstado : byte
{
    EnTransito_Planta = 10,
    Descargando = 20,
    Procesado = 30,
    Cerrado = 40,
    Cancelado = 90
}
