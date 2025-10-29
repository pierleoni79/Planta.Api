// Ruta: /Planta.Data/Entities/ReciboEstadoLog.cs | V1.2
#nullable enable
using System;
using System.ComponentModel.DataAnnotations;        // MaxLength
using System.ComponentModel.DataAnnotations.Schema; // Table, ForeignKey

namespace Planta.Data.Entities
{
    /// <summary>
    /// Bitácora de cambios de estado de un Recibo (op.ReciboEstadoLog).
    /// </summary>
    [Table("ReciboEstadoLog", Schema = "op")]
    public sealed class ReciboEstadoLog
    {
        public long Id { get; set; }                    // bigint IDENTITY
        public Guid ReciboId { get; set; }              // FK → op.Recibo.Id

        public byte Estado { get; set; }                // tinyint (se castea a enum en capa Contracts si aplica)
        public DateTimeOffset Cuando { get; set; }      // DEFAULT sysdatetimeoffset()

        [MaxLength(512)]
        public string? Nota { get; set; }               // nvarchar(512) NULL

        [MaxLength(128)]
        public string? GPS { get; set; }                // nvarchar(128) NULL

        // Navegación (opcional); relación configurada en ReciboEstadoLogConfig con DeleteBehavior.Restrict
        [ForeignKey(nameof(ReciboId))]
        public Recibo? Recibo { get; set; }
    }
}
