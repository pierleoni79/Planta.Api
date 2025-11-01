// Ruta: /Planta.Api/Utilities/ETagUtils.cs | V1.0
#nullable enable
namespace Planta.Api.Utilities;

/// <summary>Utilidades para calcular ETags coherentes con la BD.</summary>
public static class ETagUtils
{
    /// <summary>ETag débil: W/"{id:N}-{yyyyMMddHHmmss}"</summary>
    public static string ForRecibo(Guid id, DateTime lastUpdateUtc)
    {
        return $"W/\"{id:N}-{lastUpdateUtc:yyyyMMddHHmmss}\"";
    }

    /// <summary>ETag para listados basado en los ETag individuales y el total.</summary>
    public static string ForList(IEnumerable<string> itemEtags, int total)
    {
        using var sha = SHA256.Create();
        var joined = string.Join("|", itemEtags) + $"|{total}";
        var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(joined)));
        return $"W/\"RList-{hash[..16]}\""; // corto
    }
}
