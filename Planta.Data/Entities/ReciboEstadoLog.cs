using System;

namespace Planta.Data.Entities;

public class ReciboEstadoLog
{
    public long Id { get; set; }                // bigint identity
    public Guid ReciboId { get; set; }          // uniqueidentifier
    public byte Estado { get; set; }            // tinyint
    public DateTimeOffset Cuando { get; set; }  // default: sysdatetimeoffset()
    public string? Nota { get; set; }           // nvarchar(512) NULL
    public string? GPS { get; set; }            // nvarchar(128) NULL
}
