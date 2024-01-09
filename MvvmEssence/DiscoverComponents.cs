#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Brain2CPU.MvvmEssence;

public class DiscoverComponents
{
    private readonly List<ClassInterface> _singletonTypes = new();
    private readonly List<ClassInterface> _transientTypes = new();

    public ReadOnlyCollection<(ClassInterface type, bool isSingleton)> Types
    {
        get
        {
            var result = new List<(ClassInterface type, bool isSingleton)>();
            result.AddRange(_singletonTypes.Select(x => (x, true)));
            result.AddRange(_transientTypes.Select(x => (x, false)));
            return result.AsReadOnly();
        }
    }

    // use explicitly marked classes only
    public DiscoverComponents(Assembly assembly) => Analyze(assembly, null, null);

    // use filters, attributes have priority
    public DiscoverComponents(Assembly assembly, Func<Type, ClassRegistrationOption>? predicate) => Analyze(assembly, null, predicate);
    
    public DiscoverComponents(Assembly assembly, Func<string, string, ClassRegistrationOption>? predicate) => Analyze(assembly, predicate, null);

    private void Analyze(Assembly assembly, Func<string, string, ClassRegistrationOption>? stringPredicate, Func<Type, ClassRegistrationOption>? typePredicate)
    {
        var interfaces = assembly.GetTypes().Where(x => x.IsInterface).ToList();

        foreach (var type in assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !string.IsNullOrEmpty(x.Namespace) && !x.Name.Contains("<")))
        {
            var (addTo, iName) = GetCategory(type, stringPredicate, typePredicate);

            if (addTo == null)
                continue;

            Type? iType = iName switch
            {
                null => type.GetInterfaces().FirstOrDefault(x => interfaces.Contains(x)),
                "" => null,
                _ => type.GetInterfaces().FirstOrDefault(x => string.Equals(x.Name, iName, StringComparison.Ordinal))
            };

            addTo.Add(new ClassInterface(type, iType));
        }
    }

    private (List<ClassInterface>? list, string? interfaceName) GetCategory(Type type, Func<string, string, ClassRegistrationOption>? stringPredicate, Func<Type, ClassRegistrationOption>? typePredicate)
    {
        if (type.GetCustomAttribute<SkipRegistrationAttribute>() != null)
            return (null, null);

        string? interfaceName = null;  // use any available
        if (type.GetCustomAttribute<IgnoreInterfaceForRegistrationAttribute>() != null)
            interfaceName = "";        // ignore
        else
        {
            var i = type.GetCustomAttribute<UseInterfaceForRegistrationAttribute>();
            if (i != null)
                interfaceName = i.Name;  // use specified
        }

        if (type.GetCustomAttribute<RegisterAsTransientAttribute>() != null)
            return (_transientTypes, interfaceName);

        if (type.GetCustomAttribute<RegisterAsSingletonAttribute>() != null)
            return (_singletonTypes, interfaceName);

        if (stringPredicate == null && typePredicate == null) 
            return (null, null);
        
        return (stringPredicate?.Invoke(type.Namespace, type.Name) ?? typePredicate?.Invoke(type)) switch
        {
            ClassRegistrationOption.AsSingleton => (_singletonTypes, interfaceName),
            ClassRegistrationOption.AsTransient => (_transientTypes, interfaceName),
            _ => (null, null)
        };
    }

    public void RegisterItems(Action<Type> singletonDirectHandler, Action<Type> transientDirectHandler, Action<Type, Type> singletonHandler, Action<Type, Type> transientHandler)
    {
        foreach (var type in _singletonTypes)
        {
            if (type.Interface == null)
                singletonDirectHandler?.Invoke(type.Class);
            else
                singletonHandler?.Invoke(type.Class, type.Interface);
        }

        foreach (var type in _transientTypes)
        {
            if (type.Interface == null)
                transientDirectHandler?.Invoke(type.Class);
            else
                transientHandler?.Invoke(type.Class, type.Interface);
        }
    }

    // ignores interfaces, register the class directly
    public void RegisterItems(Action<Type> singletonDirectHandler, Action<Type> transientDirectHandler)
    {
        foreach (var type in _singletonTypes)
        {
            singletonDirectHandler?.Invoke(type.Class);
        }

        foreach (var type in _transientTypes)
        {
            transientDirectHandler?.Invoke(type.Class);
        }
    }
}

public record struct ClassInterface(Type Class, Type? Interface);

public enum ClassRegistrationOption
{
    Skip, 
    AsSingleton,
    AsTransient
}

[AttributeUsage(AttributeTargets.Class)]
public class IgnoreInterfaceForRegistrationAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class UseInterfaceForRegistrationAttribute : Attribute
{
    public string Name { get; }
    public UseInterfaceForRegistrationAttribute(string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Class)]
public class SkipRegistrationAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class RegisterAsSingletonAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class RegisterAsTransientAttribute : Attribute
{
}
