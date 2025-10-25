// Ruta: /Planta.Domain/Operaciones/ReciboEstadoLog.cs | V1.0
using System;

namespace Planta.Domain.Operaciones
{
    // Mapea a op.ReciboEstadoLog
    public sealed class ReciboEstadoLog
    {
        public long Id { get; set; }               // bigint identity
        public Guid ReciboId { get; set; }         // uniqueidentifier
        public byte Estado { get; set; }           // tinyint
        public DateTimeOffset Cuando { get; set; } // default BD: sysdatetimeoffset()
        public string? Nota { get; set; }          // nvarchar(512)
        public string? GPS { get; set; }           // nvarchar(128) "lat,lng;acc=..;src=..;dev=.."
    }
}
