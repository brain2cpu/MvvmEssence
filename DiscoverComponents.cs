using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Brain2CPU.MvvmEssence;

public class DiscoverComponents
{
    private readonly List<Type> _singletonTypes = new();
    private readonly List<Type> _transientTypes = new();

    public ReadOnlyCollection<(Type type, bool isSingleton)> Types
    {
        get
        {
            var result = new List<(Type type, bool isSingleton)>();
            result.AddRange(_singletonTypes.Select(x => (x, true)));
            result.AddRange(_transientTypes.Select(x => (x, false)));
            return result.AsReadOnly();
        }
    }

    //use only explicitly marked classes
    public DiscoverComponents(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if(type.GetCustomAttribute<RegisterAsTransientAttribute>() != null)
                _transientTypes.Add(type);
            else if(type.GetCustomAttribute<RegisterAsSingletonAttribute>() != null)
                _singletonTypes.Add(type);
        }
    }
    
    //use filters, attributes has priority
    public DiscoverComponents(Assembly assembly, bool useDefaultAsSingleton, string[] limitToNamespaces, string[] suffixes)
    {
        if (limitToNamespaces == null)
            throw new ArgumentNullException(nameof(limitToNamespaces));
                                                                          // to skip some generated code
        foreach (var type in assembly.GetTypes().Where(x => x.IsClass && !x.Name.Contains("<")))
        {
            if(type.GetCustomAttribute<RegisterAsTransientAttribute>() != null)
                _transientTypes.Add(type);
            
            else if(type.GetCustomAttribute<RegisterAsSingletonAttribute>() != null)
                _singletonTypes.Add(type);
            
            else if (limitToNamespaces.Contains(type.Namespace) && 
                (suffixes == null || suffixes.Any(x => type.Name.EndsWith(x, StringComparison.Ordinal))))
            {
                if(useDefaultAsSingleton)
                    _singletonTypes.Add(type);
                else
                    _transientTypes.Add(type);
            }
        }
    }

    //use lambda filters, attributes has priority
    public DiscoverComponents(Assembly assembly, Func<Type, ClassRegistrationOption> predicate)
    {
        foreach (var type in assembly.GetTypes().Where(x => x.IsClass))
        {
            if (type.GetCustomAttribute<RegisterAsTransientAttribute>() != null)
                _transientTypes.Add(type);

            else if (type.GetCustomAttribute<RegisterAsSingletonAttribute>() != null)
                _singletonTypes.Add(type);

            else 
            {
                switch(predicate(type))
                {
                    case ClassRegistrationOption.AsSingleton:
                        _singletonTypes.Add(type);
                        break;

                    case ClassRegistrationOption.AsTransient:
                        _transientTypes.Add(type);
                        break;
                }
            }
        }
    }

    public void RegisterItems(Action<Type> singletonHandler, Action<Type> transientHandler)
    {
        foreach (var type in _singletonTypes)
        {
            singletonHandler(type);
        }

        foreach (var type in _transientTypes)
        {
            transientHandler(type);
        }
    }
}

public enum ClassRegistrationOption
{
    Skip, 
    AsSingleton,
    AsTransient
}

[AttributeUsage(AttributeTargets.Class)]
public class RegisterAsSingletonAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class RegisterAsTransientAttribute : Attribute
{
}