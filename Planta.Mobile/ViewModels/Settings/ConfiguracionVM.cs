// Ruta: /Planta.Mobile/ViewModels/Settings/ConfiguracionVM.cs | V1.1-fix
using Planta.Mobile.Services.Api;
using Planta.Mobile.ViewModels.Base;
using static Android.Icu.Text.CaseMap;

namespace Planta.Mobile.ViewModels;

public sealed class ConfiguracionVM : BaseViewModel
{
    private readonly IApiClient _api;

    public string BaseUrl
    {
        get => _api.BaseUrl;
        set
        {
            if (_api.BaseUrl != value)
            {
                _api.BaseUrl = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }

    public ConfiguracionVM(IApiClient api)
    {
        Title = "Configuración";
        _api = api;
    }
}
