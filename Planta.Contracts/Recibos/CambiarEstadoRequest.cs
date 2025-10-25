// Ruta: /Planta.Contracts/Recibos/CambiarEstadoRequest.cs | V1.0
namespace Planta.Contracts.Recibos;

public sealed class CambiarEstadoRequest
{
    public ReciboEstado NuevoEstado { get; set; }
    public string? Comentario { get; set; }
}
