// Ruta: /Planta.Contracts/Recibos/CheckinRequest.cs | V1.2
namespace Planta.Contracts.Recibos
{
    /// <summary>
    /// Payload para check-in manual en planta.
    /// </summary>
    public sealed class CheckinRequest
    {
        /// <summary>
        /// Texto opcional con ubicación. Ej.: "6.25184,-75.56359 ±10m @ 2025-10-25T15:30:00Z [manual]".
        /// Puede venir vacío en check-in manual.
        /// </summary>
        public string? Gps { get; set; }

        /// <summary>
        /// Nota/comentario opcional (se guarda en el recibo y en el log de estado).
        /// </summary>
        public string? Comentario { get; set; }
    }
}
