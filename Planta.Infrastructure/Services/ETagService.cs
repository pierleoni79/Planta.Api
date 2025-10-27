// Planta.Infrastructure / Services / ETagService.cs | V1.2
#nullable enable
using System.Text;
using Planta.Application.Abstractions;
using Planta.Domain.Recibos;

namespace Planta.Infrastructure.Services;

internal sealed class ETagService : IETagService
{
    public string ComputeETag(Recibo r)
    {
        var stamp = r.UltimaActualizacion ?? r.FechaCreacion;
        var raw = $"{r.Consecutivo}|{stamp.UtcDateTime:o}|{(byte)r.Estado}";
        return $"W/\"{System.Convert.ToBase64String(Encoding.UTF8.GetBytes(raw))}\"";
    }

    public bool Matches(string? ifMatchHeader, Recibo r)
        => !string.IsNullOrWhiteSpace(ifMatchHeader)
           && string.Equals(ifMatchHeader.Trim(), ComputeETag(r), System.StringComparison.Ordinal);
}
