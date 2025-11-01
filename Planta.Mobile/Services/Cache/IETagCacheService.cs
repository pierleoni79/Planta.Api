// Ruta: /Planta.Mobile/Services/Cache/IETagCacheService.cs | V1.0
namespace Planta.Mobile.Services.Cache;

public interface IETagCacheService
{
    string? Get(string url);
    void Put(string url, string etag);
    void Remove(string url);
}
