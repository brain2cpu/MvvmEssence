# MvvmEssence

This is a .NET standard 2.0 library, can be used in WPF, Xamarin Forms and MAUI projects as well.
To help the adoption of this library a sample MAUI application was added, the few existing unit tests could also help.

### Usage

Define your ViewModel class as:

```CS
using Brain2CPU.MvvmEssence;

public class MyViewModel : ViewModelBase
```

In the XAML file do the binding in the usual way. In the ViewModel instead of the traditional way to declare a property used in binding or a command:
```CS
private DateTime _dateFrom = DateTime.Today;
public DateTime DateFrom
{
    get => _dateFrom;
    set
    {
        if (_dateFrom == value)
            return;

        _dateFrom = value;
        NotifyPropertyChanged();
        RefreshCommand.ChangeCanExecute();
    }

private Xamarin.Forms.Command _refreshCommand;
public Xamarin.Forms.Command RefreshCommand => 
    _refreshCommand ?? (_refreshCommand = new Command(Refresh, () => !IsBusy && DateFrom > DateTime.Now));
}
````

use the following:
```CS
public DateTime DateFrom
{
    get => Get(defaultVal:DateTime.Today);
    set => Set(value, RefreshCommand);
}

public RelayCommand RefreshCommand => Get(Refresh, () => !IsBusy && DateFrom > DateTime.Now);
```

with the same
```CS
private void Refresh() { ... }
```

```ViewModelBase``` will take care of the storage and notifications.

The ```Get``` method used in the property definition must have specified the generic type or the default value.
The ```Set``` method has an optional command list parameter, for commands that must update their CanExecute state based on the property value. 

```Set``` supports a custom comparer too:

```CS
public DateTime DateTo
{
    get => Get<DateTime>();   //will have the type's default value
    set => Set(value, RefreshCommand, (d1, d2) => d1.Date == d2.Date);
}
```

The ```Get``` method used in the command definition has a first parameter of type ```Action``` or ```Action<T>``` and an optional canExecute ```Func<bool>``` parameter.

For the case of an asynchronous method servicing a command, instead of a fire-and-forget approach ```async void Handler()``` you can use ```RelayCommandAsync``` with a 
proper ```async Task HandlerAsync()```. Keep in mind that this is a syntactic sugar, the execution of the method won't be awaited, so handle all exceptions inside the method!

The ```IsBusy``` property setter will call ```RaiseCanExecuteChanged``` for every command.

There is a ```Get``` overload having a delegate parameter ```bool IsValid(object o)```, in this case every ```set``` will trigger a validaton, 
the global result is in the ```IsObjectValid ``` property, a collection of the names of the invalid properties is stored in ```InvalidFields```. 
If a validation againts the default values are required you must call the ```InitializeObject()``` method.

In order to help in situations where asynchronous initialization is required during the view model construction there is a virtual method ```InitializeAsync``` 
called from the default constructor. **Important**: please pay attention to the fact that ```StartInitialization``` is called before the child class 
constructor is even started, so the auto-initialized constructor can be used only if the child constructor is empty or whatever initialization is done in it 
will not affect the async initialization. Otherwise you should use the constructor with a boolean parameter and start the initialization in the child class. 
The end of initialization can be observed using the ```IsInitialized``` property or/and by subscribing to the ```OnInitialized``` event.

### ObservableCollectionEx

This class is not strictly required for the MVVM pattern but it is very useful in many situations. It extends ```ObservableCollection``` restricting it to 
observable items and allows temporary notification suppression and range operations (```AddRange```, ```RemoveRange```). 
The main addition though is that the ```OnCollectionChanged``` event is triggered not only when the collection is changed (add/remove items) but 
also when an observable property of any contained item is modified.

### DiscoverComponents

Another addition to the package, used mainly to help component registration in the MAUI DI engine.
```CS
var discover = new DiscoverComponents(typeof(MauiProgram).Assembly, 
            false, 
            new[] { "MyNamespace.Services", "MyNamespace.Views.*", "MyNamespace.ViewModels.*" }, 
            new[] { "Service", "Page", "ViewModels" });
discover.RegisterItems(s => builder.Services.AddSingleton(s), 
            t => builder.Services.AddTransient(t));
```
You can use the registration based on naming conventions, a selector method or explicitly marked classes with ```RegisterAsTransientAttribute``` or ```RegisterAsSingletonAttribute```.

