using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Brain2CPU.MvvmEssence;

public class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private readonly Dictionary<string, PropertyInfo> _propertyInfo;

    public Transaction Transaction { get; }

    protected ObservableObject()
    {
        _propertyInfo = GetType()
            .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToDictionary(x => x.Name);

        // Executes get for all public read-write properties to initialize them
        foreach (var propertyInfo in _propertyInfo.Values)
        {
            propertyInfo.GetValue(this);
        }

        Transaction = new Transaction(this);
    }

    // Property storage
    private readonly Dictionary<string, object?> _fieldValues = [];
    
    // Validation system
    private readonly Dictionary<string, IPropertyValidator[]> _propertyValidators = [];
    private readonly HashSet<string> _nonValidatedProperties = [];
    private readonly Dictionary<string, List<string>> _validationErrors = [];
    
    // Change tracking
    private readonly HashSet<string> _changedFields = [];

    #region Validation Properties

    /// <summary>
    /// Gets whether the object has any validation errors
    /// </summary>
    public bool IsObjectValid => !HasValidationErrors;

    /// <summary>
    /// Gets whether there are any validation errors
    /// </summary>
    public bool HasValidationErrors => _validationErrors.Any(kvp => kvp.Value.Count > 0);

    /// <summary>
    /// Gets all validation errors for all properties
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ValidationErrors => 
        _validationErrors
            .Where(x => x.Value.Count > 0)
            .ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly());

    /// <summary>
    /// Gets validation errors for a specific property
    /// </summary>
    public IReadOnlyList<string> GetValidationErrors(string propertyName)
    {
        return _validationErrors.TryGetValue(propertyName, out var errors) 
            ? errors.AsReadOnly() 
            : new List<string>().AsReadOnly();
    }

    /// <summary>
    /// Gets property names that have validation errors (legacy compatibility)
    /// </summary>
    public IReadOnlyList<string> InvalidFields => 
        _validationErrors.Where(kvp => kvp.Value.Count > 0).Select(kvp => kvp.Key).ToList();

    /// <summary>
    /// Checks if a specific property is valid
    /// </summary>
    public bool IsPropertyValid(string propertyName) => !_validationErrors.ContainsKey(propertyName) || 
                                                        _validationErrors[propertyName].Count == 0;

    /// <summary>
    /// Event raised when validation errors change
    /// </summary>
    public event EventHandler<string>? ValidationErrorsChanged;

    #endregion

    #region Change Tracking Properties

    /// <summary>
    /// Gets whether any properties have been changed
    /// </summary>
    public bool IsChanged => _changedFields.Count != 0;

    /// <summary>
    /// Checks if a specific property has been changed
    /// </summary>
    public bool IsPropertyChanged(string name) => _changedFields.Contains(name);

    /// <summary>
    /// Gets the names of all changed properties
    /// </summary>
    public IReadOnlyList<string> ChangedFields => _changedFields.ToList();

    /// <summary>
    /// Resets the change tracking state
    /// </summary>
    public void ResetChanges()
    {
        _changedFields.Clear();
        NotifyPropertyChanged(nameof(IsChanged));
    }

    #endregion

    #region Property Access Methods

    protected T Get<T>([CallerMemberName] string? propertyName = null) => 
        Get(default(T), propertyName);

    protected T Get<T>(T defaultVal, [CallerMemberName] string? propertyName = null)
    {
        if(_fieldValues.TryGetValue(propertyName!, out object? v))
            return (T)v;

        _fieldValues.Add(propertyName!, defaultVal);

        ValidateProperty(propertyName!, defaultVal);

        return defaultVal;
    }

    protected delegate bool EqualityChecker<in T>(T t1, T t2);

    /// <summary>
    /// Sets a property value with validation
    /// </summary>
    protected bool Set<T>(T value, EqualityChecker<T>? equalityChecker = null, [CallerMemberName] string? propertyName = null)
    {
        if(Transaction.ExecutingUndo)
        {
            _fieldValues[propertyName!] = value;
            NotifyPropertyChanged(propertyName);
            AddChange(propertyName!);

            return true;
        }

        ValidateProperty(propertyName!, value);

        if (_fieldValues.TryGetValue(propertyName!, out object? v))
        {
            if (equalityChecker?.Invoke((T)v, value) ?? EqualityComparer<T>.Default.Equals((T)v, value))
                return false;

            Transaction.RecordChange(propertyName!, v, value);
            _fieldValues[propertyName!] = value;
        }
        else
        {
            Transaction.RecordChange(propertyName!, default(T), value);
            _fieldValues.Add(propertyName!, value);
        }

        NotifyPropertyChanged(propertyName);
        AddChange(propertyName!);

        return true;
    }

    /// <summary>
    /// Sets a property value using backing field with validation
    /// </summary>
    protected bool Set<T>(ref T storage, T value, EqualityChecker<T>? equalityChecker = null, 
        [CallerMemberName] string? propertyName = null)
    {
        if (Transaction.ExecutingUndo)
        {
            storage = value;
            NotifyPropertyChanged(propertyName);
            AddChange(propertyName!);

            return true;
        }

        ValidateProperty(propertyName!, value);

        if (equalityChecker?.Invoke(storage, value) ?? EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        Transaction.RecordChange(propertyName!, storage, value);
        storage = value;

        NotifyPropertyChanged(propertyName);
        AddChange(propertyName!);

        return true;
    }

    #endregion

    #region Validation Methods

    protected void AddValidator(string propertyName, IPropertyValidator validator)
    {
        if (_propertyValidators.ContainsKey(propertyName))
            _propertyValidators[propertyName] = _propertyValidators[propertyName].Append(validator).ToArray();
        else
            _propertyValidators[propertyName] = [validator];
        
        _nonValidatedProperties.Remove(propertyName);
    }

    private IPropertyValidator[] DiscoverAttributeValidators(string propertyName)
    {
        // property is already verified and it is not validated OR does not exist then nothing to do
        if(_nonValidatedProperties.Contains(propertyName) || !_propertyInfo.TryGetValue(propertyName, out var property))
            return [];

        // validators already found
        if (_propertyValidators.TryGetValue(propertyName, out var validators))
            return validators;

        // discover validators now
        var attributeValidators = property.GetCustomAttributes(typeof(ValidationAttribute), true)
                                        .Cast<ValidationAttribute>()
                                        .Cast<IPropertyValidator>()
                                        .ToArray();
        if (attributeValidators.Length > 0)
            _propertyValidators.Add(propertyName, attributeValidators);
        else
            _nonValidatedProperties.Add(propertyName);

        return attributeValidators;
    }

    private void ValidateProperty(string propertyName, object? value)
    {
        var validators = DiscoverAttributeValidators(propertyName);
        if (validators.Length == 0)
            return;

        bool hadErrors;
        if (_validationErrors.TryGetValue(propertyName, out var errors))
        {
            hadErrors = errors.Count > 0;
            errors.Clear();
        }
        else
        {
            hadErrors = false;
            _validationErrors[propertyName] = errors = [];
        }

        foreach (var validator in validators)
        {
            if (!validator.IsValid(value))
            {
                errors.Add(validator.ErrorMessage ?? "Validation failed");
            }
        }

        var hasErrors = errors.Count > 0;
        if (hadErrors != hasErrors)
        {
            NotifyPropertyChanged(nameof(IsObjectValid));
            NotifyPropertyChanged(nameof(HasValidationErrors));
            ValidationErrorsChanged?.Invoke(this, propertyName);
        }
    }

    public void ValidateAllProperties()
    {
        foreach (var property in _propertyInfo.Values)
        {
            var value = property.GetValue(this);
            ValidateProperty(property.Name, value);
        }
    }

    #endregion

    #region Helper Methods

    private void AddChange(string name)
    {
        _changedFields.Add(name); 

        if (_changedFields.Count == 1)
            NotifyPropertyChanged(nameof(IsChanged));
    }

    #endregion
}