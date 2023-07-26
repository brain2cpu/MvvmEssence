using System;
using System.Windows.Input;

namespace Brain2CPU.MvvmEssence;

public abstract class RelayCommandBase : ICommand
{
    private readonly Func<bool> _canExecute;

    public event EventHandler CanExecuteChanged;

    protected RelayCommandBase(Func<bool> canExecute)
    {
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public abstract void Execute(object parameter);
}