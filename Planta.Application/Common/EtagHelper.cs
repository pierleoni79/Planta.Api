// Ruta: /Planta.Application/Common/EtagHelper.cs | V1.0
#nullable enable
using System.Text;

namespace Planta.Application.Common;

public static class EtagHelper
{
    public static string ForRecibo(int consecutivo, DateTimeOffset? ultimaActualizacion, DateTimeOffset fechaCreacion, int estado)
    {
        var stamp = ultimaActualizacion ?? fechaCreacion;
        var raw = $"{consecutivo}|{stamp.UtcDateTime:o}|{estado}";
        return $"W/\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(raw))}\"";
    }
}
