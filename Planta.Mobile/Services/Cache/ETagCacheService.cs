// Ruta: /Planta.Mobile/Services/Cache/ETagCacheService.cs | V1.0
using System.Collections.Concurrent;

namespace Planta.Mobile.Services.Cache;

public sealed class ETagCacheService : IETagCacheService
{
    private readonly ConcurrentDictionary<string, string> _mem = new();
    public string? Get(string url) => _mem.TryGetValue(url, out var v) ? v : null;
    public void Put(string url, string etag) => _mem[url] = etag;
    public void Remove(string url) => _mem.TryRemove(url, out _);
}
