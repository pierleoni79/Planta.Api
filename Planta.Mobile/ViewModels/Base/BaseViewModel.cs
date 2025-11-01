// Ruta: /Planta.Mobile/ViewModels/Base/BaseViewModel.cs | V1.0
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Planta.Mobile.ViewModels.Base;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    bool isBusy;
    string? title;

    public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value); }
    public string? Title { get => title; set => SetProperty(ref title, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(name);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
