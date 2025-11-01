// Ruta: /Planta.Mobile/App.xaml.cs | V1.2-fix (DI en App + Shell con SP + listo para modal de inicio)
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Planta.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _sp;

    public App(IServiceProvider sp)
    {
        InitializeComponent();
        _sp = sp;
        // ❌ No establezcas MainPage aquí; la creamos en CreateWindow.
    }

    // Punto de entrada UI (single-window). Inyecta el ServiceProvider al AppShell.
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell(_sp); // ← AppShell debe tener ctor(AppShell(IServiceProvider sp))
        var window = new Window(shell)
        {
            Title = "Caliza - Operario"
        };
        return window;
    }
}
