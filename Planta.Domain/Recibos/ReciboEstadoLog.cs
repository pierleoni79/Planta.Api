// Ruta: /Planta.Domain/Recibos/ReciboEstadoLog.cs | V1.1 (alineado a BD)
#nullable enable
namespace Planta.Domain.Recibos;

using Planta.Domain.Common;

public sealed class ReciboEstadoLog : Entity<long>
{
    // PK bigint identity (Id heredado de Entity<long>)
    public Guid ReciboId { get; private set; }

    // tinyint en BD → se mapeará con .HasConversion<byte>() en la Config
    public int Estado { get; private set; }

    // datetimeoffset en BD
    public DateTimeOffset Cuando { get; private set; }

    // nvarchar(256) y nvarchar(64) en BD (ambos NULL)
    public string? Nota { get; private set; }
    public string? Gps { get; private set; }

    private ReciboEstadoLog() : base(0) { }

    public ReciboEstadoLog(
        long id,
        Guid reciboId,
        int estado,
        DateTimeOffset cuando,
        string? nota = null,
        string? gps = null) : base(id)
    {
        ReciboId = reciboId;
        Estado = estado;
        Cuando = cuando;
        Nota = nota;
        Gps = gps;
    }

    // Helper de conveniencia para “ahora”
    internal static ReciboEstadoLog NewNow(Guid reciboId, int estado, string? nota = null, string? gps = null)
        => new(0, reciboId, estado, DateTimeOffset.UtcNow, nota, gps);
}
