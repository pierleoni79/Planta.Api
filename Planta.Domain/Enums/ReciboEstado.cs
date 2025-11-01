// Ruta: /Planta.Domain/Enums/ReciboEstado.cs | V1.0
#nullable enable
namespace Planta.Domain.Enums;

using Planta.Domain.Common;

public sealed class ReciboEstado : Enumeration
{
    public static readonly ReciboEstado EnTransitoPlanta = new(10, nameof(EnTransitoPlanta));
    public static readonly ReciboEstado Descargando = new(12, nameof(Descargando));
    public static readonly ReciboEstado Procesado = new(20, nameof(Procesado));
    public static readonly ReciboEstado Cerrado = new(30, nameof(Cerrado));
    public static readonly ReciboEstado Anulado = new(90, nameof(Anulado));
    public static readonly ReciboEstado Error = new(99, nameof(Error));
    private ReciboEstado(int id, string name) : base(id, name) { }
}
