//Ruta: /Planta.Mobile/Services/ApiClientFactory.cs | V1.0
#nullable enable
using System.Net.Http;
using Microsoft.Maui.Storage;

namespace Planta.Mobile.Services;

public interface IApiClientFactory { HttpClient Create(); }

public sealed class ApiClientFactory : IApiClientFactory
{
    public HttpClient Create()
    {
        var baseUrl = Preferences.Get("ApiBaseUrl", "http://10.0.2.2:5122/");
        if (!baseUrl.EndsWith('/')) baseUrl += "/";
        var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        http.Timeout = TimeSpan.FromSeconds(30);
        return http;
    }
}