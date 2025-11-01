// Ruta: /Planta.Mobile/ViewModels/Base/RelayCommand.cs | V1.0
using System.Windows.Input;

namespace Planta.Mobile.ViewModels.Base;

public sealed class RelayCommand : ICommand
{
    private readonly Func<bool>? _canExecute;
    private readonly Func<Task>? _executeAsync;
    private readonly Action? _execute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute; _canExecute = canExecute;
    }
    public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync; _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object? parameter)
    {
        if (_execute is not null) _execute();
        else if (_executeAsync is not null) await _executeAsync();
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
