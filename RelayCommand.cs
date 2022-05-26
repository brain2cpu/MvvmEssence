using System;

namespace Brain2CPU.MvvmEssence;

public class RelayCommand : RelayCommandBase
{
    private readonly Action _execute;

    public RelayCommand(Action execute, Func<bool> canExecute = null) : base(canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public override void Execute(object parameter) => _execute();

    public void Execute() => _execute();
}

public class RelayCommand<T> : RelayCommandBase
{
    private readonly Action<T> _execute;

    public RelayCommand(Action<T> execute, Func<bool> canExecute = null) : base(canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public override void Execute(object parameter) => _execute((T)parameter);
}