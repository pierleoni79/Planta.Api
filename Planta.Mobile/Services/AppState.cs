// Ruta: /Planta.Mobile/Services/AppState.cs | V1.0 (nuevo)
namespace Planta.Mobile.Services;

public sealed class AppState
{
    public int EmpresaId { get; set; } = 1;        // TODO: setear en login
    public string? BearerToken { get; private set; }
    public void SetToken(string? token) => BearerToken = token;
}
