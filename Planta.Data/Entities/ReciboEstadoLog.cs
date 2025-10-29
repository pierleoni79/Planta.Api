// Ruta: /Planta.Data/Entities/ReciboEstadoLog.cs | V1.1
#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planta.Data.Entities
{
    /// <summary>
    /// Bitácora de cambios de estado de un Recibo (op.ReciboEstadoLog).
    /// </summary>
    [Table("ReciboEstadoLog", Schema = "op")]
    public sealed class ReciboEstadoLog
    {
        public long Id { get; set; }                 // bigint IDENTITY
        public Guid ReciboId { get; set; }           // uniqueidentifier (FK → op.Recibo.Id)
        public byte Estado { get; set; }             // tinyint (map a enum si quieres)
        public DateTimeOffset Cuando { get; set; }   // DEFAULT (sysdatetimeoffset())
        public string? Nota { get; set; }            // nvarchar(512) NULL
        public string? GPS { get; set; }             // nvarchar(128) NULL

        // Navegación opcional (por convención usa ReciboId como FK)
        public Recibo? Recibo { get; set; }
    }
}
