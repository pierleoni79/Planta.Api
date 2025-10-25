// Ruta: /Planta.Api/Utils/ETag.cs | V1.0
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Planta.Api.Utils;

public static class ETag
{
    static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static string For(object? value)
    {
        var json = JsonSerializer.Serialize(value, JsonOpts);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        return $"W/\"{Convert.ToBase64String(hash)}\""; // weak ETag (suficiente para catálogos)
    }

    /// <summary>
    /// Coloca ETag/Cache-Control y devuelve true si debe responder 304.
    /// </summary>
    public static bool TryHandleConditionalGet(HttpRequest req, HttpResponse res, string etag)
    {
        res.Headers.ETag = etag;
        res.Headers.CacheControl = "public, max-age=60, must-revalidate";
        res.Headers.Vary = "Accept, Accept-Encoding";
        if (req.Headers.TryGetValue("If-None-Match", out var inm) && inm.Contains(etag))
        {
            res.StatusCode = StatusCodes.Status304NotModified;
            return true;
        }
        return false;
    }
}
