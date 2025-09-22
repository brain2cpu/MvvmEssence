# MvvmEssence

[![NuGet](https://img.shields.io/nuget/v/MvvmEssence.svg)](https://www.nuget.org/packages/MvvmEssence/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**MvvmEssence** is a lightweight, high-performance MVVM framework for .NET 8 - 10, 
designed to eliminate boilerplate code while providing powerful features for modern .NET applications, 
for maximum productivity with minimal overhead

## 🚀 Key Features

- **Simplified Property Management** - No more backing fields or manual notifications
- **Command Caching** - Efficient command creation
- **Async Command Support** - Proper async/await handling with exception management
- **Command Template Support** - Avoid repeated command handler code
- **Attribute-Based Validation** - Declarative validation with comprehensive error reporting
- **Transaction / Undo Support** - Built-in transaction-based change tracking
- **Change Tracking** - Automatic dirty state monitoring
- **Enhanced Collections** - Batch operations and property change notifications
- **Dependency Injection Helper** - Automatic service registration

## 📦 Compatibility

- **.NET 8, 9 and 10**
- **WPF, MAUI.NET** - Cross-platform support

## 🏁 Quick Start

### 1. Install the Package

```bash
dotnet add package MvvmEssence
```

### 2. Create Your ViewModel

```csharp
using Brain2CPU.MvvmEssence;

public class PersonViewModel : ViewModelBase
{
    [Required]
    [StringLength(2, 50)]
    public string Name
    {
        get => Get<string>();
        set => Set(value, SaveCommand);
    }

    [Email]
    public string Email
    {
        get => Get<string>();
        set => Set(value);
    }

    [Range(0, 120)]
    public int Age
    {
        get => Get<int>();
        set => Set(value);
    }

    public RelayCommand SaveCommand => Get(() => Save(), () => !IsBusy && IsObjectValid);
    
    private void Save()
    {
        // Save logic here
    }
}
```

### 3. Use in XAML

```xml
<TextBox Text="{Binding Name}" />
<Button Command="{Binding SaveCommand}" Content="Save" />
```

## 📖 Detailed Usage

### Properties - Before vs After

**Traditional MVVM:**
```csharp
private string _name;
public string Name
{
    get => _name;
    set
    {
        if (_name == value) 
            return;
        _name = value;
        NotifyPropertyChanged();
        SaveCommand?.RaiseCanExecuteChanged();
    }
}
```

**MvvmEssence:**
```csharp
public string Name
{
    get => Get<string>();
    set => Set(value, SaveCommand);
}
```

### Commands - Before vs After

**Traditional MVVM:**
```csharp
private ICommand _saveCommand;
public ICommand SaveCommand => 
    _saveCommand ??= new RelayCommand(Save, () => !IsBusy && IsValid);
```

**MvvmEssence:**
```csharp
public RelayCommand SaveCommand => Get(() => Save(), () => !IsBusy && IsValid);
```

### Async Commands

```csharp
public RelayCommandAsync LoadDataCommand => Get(LoadDataAsync, () => !IsBusy);

private async Task LoadDataAsync()
{
    // Async operation
    await SomeApiCall();
}
```

### Exception Handling

```csharp
public MyViewModel()
{
    // Global exception handler for templated commands
    ExceptionHandler = ex => 
    {
        Debug.WriteLine($"Command error: {ex.Message}");
        // Handle or log exception
    };

    public RelayCommand<string?> ChangeNameCommand =>
        GetTemplated<string?>(ChangeName, () => !IsBusy && !string.IsNullOrEmpty(Name));
}
```

## ✅ Validation System

### Attribute-Based Validation

```csharp
public class UserRegistrationViewModel : ViewModelBase
{
    [Required()]
    [StringLength(3, 20)]
    public string Username
    {
        get => Get<string>();
        set => Set(value);
    }

    [Required]
    [Email]
    public string Email
    {
        get => Get<string>();
        set => Set(value);
    }

    [Required]
    [StringLength(8, 100)]
    [Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])")]
    public string Password
    {
        get => Get<string>();
        set => Set(value);
    }

    // Validation state properties
    public bool IsValid => IsObjectValid;
    public string ValidationSummary => string.Join(", ", 
        ValidationErrors.SelectMany(kvp => kvp.Value));
}
```

### Available Validation Attributes

- **`[Required]`** - Property cannot be null or empty
- **`[Number<T>]`** - Value must be a number of the specified type
- **`[Range<T>(min, max)]`** - Numeric range validation
- **`[Between<T>(min, max)]`** - IComparable range validation
- **`[StringLength(min, max)]`** - String length validation
- **`[Email]`** - Email format validation
- **`[Date]`** - DateOnly validation
- **`[DateTime]`** - DateTime validation
- **`[Regex(pattern)]`** - Regular expression validation
- **`[Enum<T>]`** - Value must be from the specified enum
- **`[CustomValidation<T>]`** - T must implement `IValidator`, used for complex scenarios


## ↩️ Transaction / Undo Support

```csharp
public class DocumentViewModel : ObservableObject
{
    public string Title
    {
        get => Get<string>();
        set => Set(value);
    }

    public string Content
    {
        get => Get<string>();
        set => Set(value);
    }

    // Grouped operations
    public void UpdateDocument(string title, string content)
    {
        Transaction.Begin();
        Title = title;
        Content = content;
        Transaction.Commit();
    }

    public RelayCommand UndoCommand => Get(() => Transaction.Undo(), () => CanUndo);
}
```

## 📚 Enhanced Collections

**ObservableCollectionEx** provides advanced collection features:

```csharp
public class ItemsViewModel : ViewModelBase
{
    public ObservableCollectionEx<ItemViewModel> Items { get; } = 
        new ObservableCollectionEx<ItemViewModel>();

    public void LoadItems(IEnumerable<ItemViewModel> items)
    {
        // Efficient bulk operations
        Items.SuppressNotification = true;
        Items.Clear();
        Items.AddRange(items);
        Items.SuppressNotification = false; // Single notification
    }

    public void RemoveSelected(IEnumerable<ItemViewModel> selected)
    {
        Items.RemoveRange(selected);
    }
}
```

### Key Features:
- **Batch Operations** - `AddRange()`, `RemoveRange()`
- **Notification Control** - `SuppressNotification` property
- **Item Property Changes** - Automatically propagates item property changes, restricted to `INotifyPropertyChanged`

## 🏗️ Dependency Injection Integration

**DiscoverComponents** automates service registration:

```csharp
// In MauiProgram.cs or Startup.cs
var discover = new DiscoverComponents(typeof(App).Assembly, type =>
{
    if (type.Namespace?.Contains("ViewModels") == true)
        return ClassRegistrationOption.AsTransient;
        
    if (type.Namespace?.Contains("Services") == true)
        return ClassRegistrationOption.AsSingleton;

    return ClassRegistrationOption.Skip;
});

discover.RegisterItems(
    singletonType => builder.Services.AddSingleton(singletonType),
    transientType => builder.Services.AddTransient(transientType),
    (implType, interfaceType) => builder.Services.AddSingleton(interfaceType, implType),
    (implType, interfaceType) => builder.Services.AddTransient(interfaceType, implType));
```

### Attribute-Based Registration

```csharp
[RegisterAsSingleton]
public class DataService : IDataService
{
    // Implementation
}

[RegisterAsTransient]
public class MainViewModel : ViewModelBase
{
    // ViewModel implementation
}
```

## 🔧 Advanced Features

### Custom Equality Comparers

```csharp
public DateTime DateTo
{
    get => Get<DateTime>();
    set => Set(value, RefreshCommand, (d1, d2) => d1.Date == d2.Date);
}
```

### Async Initialization

```csharp
public class DataViewModel : ViewModelBase
{
    protected override async Task InitializeAsync()
    {
        // Async initialization
        Data = await LoadDataAsync();
    }
}
```

### Change Tracking

```csharp
public class EditableViewModel : ViewModelBase
{
    public bool HasChanges => IsChanged;
    public IReadOnlyList<string> ChangedProperties => ChangedFields;
    
    public void ResetChanges()
    {
        base.ResetChanges();
    }
}
```


## 🔄 Migration Guide

### From MvvmEssence 1.3.*
The validation method is changed, the lambda based validation defined in the Set overload is not working in the
new version, you must switch to the attribute based validation. For more complex scenarios the easiest way is 
to move the existing validation code into the IsValid method of a new class implementing `IValidator`.  

### From CommunityToolkit.Mvvm

```csharp
// Before (CommunityToolkit)
[ObservableProperty]
private string _name;

// After (MvvmEssence)
public string Name
{
    get => Get<string>();
    set => Set(value);
}
```

### From Prism

```csharp
// Before (Prism)
private string _name;
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}

// After (MvvmEssence)
public string Name
{
    get => Get<string>();
    set => Set(value);
}
```

## 📚 Sample Projects

Check out the included sample applications:
- **MAUI Sample** - Complete cross-platform app demonstrating all features
- **Unit Tests** - Comprehensive test suite showing usage patterns

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
