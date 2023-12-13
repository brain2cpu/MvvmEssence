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
    public DiscoverComponents(Assembly assembly) => Analyze(assembly, null);


    // use filters, attributes have priority
    public DiscoverComponents(Assembly assembly, Func<Type, ClassRegistrationOption>? predicate) => Analyze(assembly, predicate);

    private void Analyze(Assembly assembly, Func<Type, ClassRegistrationOption>? predicate)
    {
        var interfaces = assembly.GetTypes().Where(x => x.IsInterface).ToList();

        List<ClassInterface>? addTo = null;
        foreach (var type in assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && !x.Name.Contains("<")))
        {
            if (type.GetCustomAttribute<RegisterAsTransientAttribute>() != null)
                addTo = _transientTypes;

            else if (type.GetCustomAttribute<RegisterAsSingletonAttribute>() != null)
                addTo = _singletonTypes;

            else if (predicate != null)
            {
                if (type.GetCustomAttribute<SkipRegistrationAttribute>() != null)
                    continue;

                switch (predicate(type))
                {
                    case ClassRegistrationOption.AsSingleton:
                        addTo = _singletonTypes;
                        break;

                    case ClassRegistrationOption.AsTransient:
                        addTo = _transientTypes;
                        break;
                }
            }

            if (addTo == null)
                continue;

            var iType = type.GetInterfaces().FirstOrDefault(x => interfaces.Contains(x));
            addTo.Add(new ClassInterface(type, iType));
            addTo = null;
        }
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
            if(type.Interface == null)
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
