﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Brain2CPU.MvvmEssence;

public class ViewModelBase : ObservableObject
{
    public event Action OnInitialized;

    public Action<Exception> ExceptionHandler { get; set; } = null;

    // StartInitialization is called before the child class constructor is even started !!!
    // the auto-initialized constructor can be used only if the child constructor is empty or 
    // whatever initialization is done in it will not affect the async initialization
    protected ViewModelBase() : this(true)
    {
    }

    protected ViewModelBase(bool initialize)
    {
        if (initialize)
            StartInitialization();
    }

    //no backing field needed
    protected bool Set<T>(T value, RelayCommandBase command, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null) =>
        Set(value, new[] {command}, equalityChecker, propertyName);

    protected bool Set<T>(T value, IEnumerable<RelayCommandBase> commands, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null)
    {
        var b = Set(value, equalityChecker, propertyName);

        if(b)
            NotifyCommands(commands);

        return b;
    }

    //setter for backing field
    protected bool Set<T>(ref T storage, T value, RelayCommandBase command, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null) =>
        Set(ref storage, value, new[] { command }, equalityChecker, propertyName);

    protected bool Set<T>(ref T storage, T value, IEnumerable<RelayCommandBase> commands, EqualityChecker<T> equalityChecker = null, [CallerMemberName] string propertyName = null)
    {
        var b = Set(ref storage, value, equalityChecker, propertyName);

        if(b)
            NotifyCommands(commands);

        return b;
    }

    //command getters
    private readonly Dictionary<string, RelayCommandBase> _commands = new();

    protected RelayCommand Get(Action execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
    {
        if(_commands.TryGetValue(commandName!, out RelayCommandBase c))
            return (RelayCommand)c;

        var nc = new RelayCommand(execute, canExecute);
        _commands.Add(commandName, nc);
        return nc;
    }

    protected RelayCommand GetTemplated(Action execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
    {
        if (_commands.TryGetValue(commandName!, out RelayCommandBase c))
            return (RelayCommand)c;

        var nc = new RelayCommand(() => TemplateMethod(execute), canExecute);
        _commands.Add(commandName, nc);
        return nc;
    }

    private void TemplateMethod(Action execute)
    {
        IsBusy = true;

        try
        {
            execute.Invoke();
        }
        catch (Exception xcp)
        {
            if (ExceptionHandler == null)
                throw;

            ExceptionHandler?.Invoke(xcp);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected RelayCommand<T> Get<T>(Action<T> execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
    {
        if(_commands.TryGetValue(commandName!, out RelayCommandBase c))
            return (RelayCommand<T>)c;

        var nc = new RelayCommand<T>(execute, canExecute);
        _commands.Add(commandName, nc);
        return nc;
    }
    
    protected RelayCommand<T> GetTemplated<T>(Action<T> execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
    {
        if(_commands.TryGetValue(commandName!, out RelayCommandBase c))
            return (RelayCommand<T>)c;

        var nc = new RelayCommand<T>(o => TemplateMethod(execute, o), canExecute);
        _commands.Add(commandName, nc);
        return nc;
    }

    private void TemplateMethod<T>(Action<T> execute, T o)
    {
        IsBusy = true;

        try
        {
            execute.Invoke(o);
        }
        catch (Exception xcp)
        {
            if (ExceptionHandler == null)
                throw;

            ExceptionHandler?.Invoke(xcp);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected RelayCommandAsync Get(ActionAsync execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
    {
        if (_commands.TryGetValue(commandName!, out RelayCommandBase c))
            return (RelayCommandAsync)c;

        var nc = new RelayCommandAsync(execute, canExecute);
        _commands.Add(commandName, nc);
        return nc;
    }

    protected RelayCommandAsync GetTemplated(ActionAsync execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
    {
        if (_commands.TryGetValue(commandName!, out RelayCommandBase c))
            return (RelayCommandAsync)c;

        var nc = new RelayCommandAsync(async () => await TemplateMethodAsync(execute), canExecute);
        _commands.Add(commandName, nc);
        return nc;
    }

    private async Task TemplateMethodAsync(ActionAsync execute)
    {
        IsBusy = true;

        try
        {
            await execute.Invoke();
        }
        catch (Exception xcp)
        {
            if (ExceptionHandler == null)
                throw;

            ExceptionHandler?.Invoke(xcp);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected RelayCommandAsync<T> Get<T>(ActionAsync<T> execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
    {
        if (_commands.TryGetValue(commandName!, out RelayCommandBase c))
            return (RelayCommandAsync<T>)c;

        var nc = new RelayCommandAsync<T>(execute, canExecute);
        _commands.Add(commandName, nc);
        return nc;
    }

    protected RelayCommandAsync<T> GetTemplated<T>(ActionAsync<T> execute, Func<bool> canExecute = null, [CallerMemberName] string commandName = null)
    {
        if (_commands.TryGetValue(commandName!, out RelayCommandBase c))
            return (RelayCommandAsync<T>)c;

        var nc = new RelayCommandAsync<T>(async o => await TemplateMethodAsync(execute, o), canExecute);
        _commands.Add(commandName, nc);
        return nc;
    }

    private async Task TemplateMethodAsync<T>(ActionAsync<T> execute, T o)
    {
        IsBusy = true;

        try
        {
            await execute.Invoke(o);
        }
        catch (Exception xcp)
        {
            if (ExceptionHandler == null)
                throw;

            ExceptionHandler?.Invoke(xcp);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void NotifyAllCommands() => NotifyCommands(_commands.Values);

    //stock busy flag
    private bool _isBusy = false;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value)
                return;

            _isBusy = value;
            NotifyPropertyChanged();

            NotifyAllCommands();
        }
    }

    private bool _isInitialized = false;
    public bool IsInitialized
    {
        get => _isInitialized;
        private set
        {
            if (_isInitialized == value)
                return;

            _isInitialized = value;
            NotifyPropertyChanged();

            NotifyAllCommands();
        }
    }

    protected async void StartInitialization()
    {
        await InitializeAsync();
        IsInitialized = true;
        OnInitialized?.Invoke();
    }

    protected virtual Task InitializeAsync() => Task.FromResult(false);

    private static void NotifyCommands(IEnumerable<RelayCommandBase> commands)
    {
        foreach (var cmd in commands)
        {
            cmd.RaiseCanExecuteChanged();
        }
    }
}