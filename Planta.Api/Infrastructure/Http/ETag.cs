// Ruta: /Planta.Api/Infrastructure/Http/ETag.cs | V1.0
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Planta.Api.Infrastructure.Http;

public static class ETag
{
    public static string WeakFromObject(object model)
    {
        var json = JsonSerializer.Serialize(model);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return $"W/\"{Convert.ToHexString(bytes)}\"";
    }

    public static bool Matches(HttpRequest req, string etag)
        => req.Headers.IfNoneMatch.Any(x => string.Equals(x, etag, StringComparison.Ordinal));
}
