//Ruta: /Planta.Mobile/Services/EtagAndCache.cs | V1.0
#nullable enable
using System.Collections.Concurrent;
using Microsoft.Maui.Storage;

namespace Planta.Mobile.Services;

public interface IEtagStore
{
    string? Get(string key);
    void Set(string key, string etag);
}

public sealed class EtagStore : IEtagStore
{
    private readonly ConcurrentDictionary<string, string> _mem = new();
    public string? Get(string key) => _mem.TryGetValue(key, out var v) ? v : Preferences.Get($"etag:{key}", null);
    public void Set(string key, string etag)
    {
        _mem[key] = etag;
        Preferences.Set($"etag:{key}", etag);
    }
}

public interface ICacheStore
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value);
}

public sealed class JsonCacheStore : ICacheStore
{
    public async Task<T?> GetAsync<T>(string key)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, $"cache_{key}.json");
        if (!File.Exists(path)) return default;
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? default : System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }
    public async Task SetAsync<T>(string key, T value)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, $"cache_{key}.json");
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
    }
}