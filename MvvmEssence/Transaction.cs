using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Brain2CPU.MvvmEssence;

/// <summary>
/// Represents a single property change that can be undone
/// </summary>
internal sealed class PropertyChange
{
    public string PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public PropertyChange(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Represents a transaction that can contain multiple property changes
/// </summary>
public sealed class Transaction : INotifyPropertyChanged
{
    private readonly Stack<PropertyChange> _undoStack = new();
    private readonly ObservableObject _observable;

    public Transaction(ObservableObject observable) => _observable = observable;

    public event Action? UndoStateChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsTransactionStarted { get; private set; }

    public bool CanUndo => _undoStack.Count > 0;

    public bool ExecutingUndo {  get; private set; }

    public void Begin() => ResetTransaction(true);

    public void Commit() => ResetTransaction(false);

    public void RecordChange(string propertyName, object? oldValue, object? newValue)
    {
        if (!IsTransactionStarted)
            return;

        _undoStack.Push(new PropertyChange(propertyName, oldValue, newValue));
        NotifyPropertyChanged(nameof(CanUndo));
    }

    public int Undo()
    {
        if (_undoStack.Count == 0) 
            return 0;

        ExecutingUndo = true;

        var count = 0;
        try
        {
            foreach (var item in _undoStack)
            {
                SetPropertyValue(item.PropertyName, item.OldValue);
                count++;
            }
        }
        finally
        {
            ResetTransaction(false);
            ExecutingUndo = false;
        }

        return count;
    }

    private void ResetTransaction(bool startTransaction)
    {
        IsTransactionStarted = startTransaction;
        _undoStack.Clear();
        
        UndoStateChanged?.Invoke();

        NotifyPropertyChanged(nameof(CanUndo));
        NotifyPropertyChanged(nameof(IsTransactionStarted));
    }

    private void SetPropertyValue(string propertyName, object? value)
    {
        var property = _observable.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(_observable, value);
        }
    }

    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
