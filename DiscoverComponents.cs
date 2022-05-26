using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Brain2CPU.MvvmEssence;

public class DiscoverComponents
{
    private readonly List<Type> _singletonTypes = new();
    private readonly List<Type> _transientTypes = new();

    public List<(Type type, bool isSingleton)> Types
    {
        get
        {
            var result = new List<(Type type, bool isSingleton)>();
            result.AddRange(_singletonTypes.Select(x => (x, true)));
            result.AddRange(_transientTypes.Select(x => (x, false)));
            return result;
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
    
    //use filters
    public DiscoverComponents(Assembly assembly, bool useDefaultAsSingleton, params string[] limitToNamespaces)
    {
        foreach (var type in assembly.GetTypes())
        {
            if(limitToNamespaces.All(x => !(type.Namespace ?? "").Contains(x)))
                continue;

            if(type.GetCustomAttribute<RegisterAsTransientAttribute>() != null)
                _transientTypes.Add(type);
            else if(type.GetCustomAttribute<RegisterAsSingletonAttribute>() != null)
                _singletonTypes.Add(type);
            else if(useDefaultAsSingleton)
                _singletonTypes.Add(type);
            else
                _transientTypes.Add(type);
        }
    }

    public void RegisterItems(Action<Type> singletonHandler, Action<Type> transientHandler, params string[] suffixes)
    {
        foreach (var type in GetItemsWithSuffix(true, suffixes))
        {
            singletonHandler(type);
        }

        foreach (var type in GetItemsWithSuffix(false, suffixes))
        {
            transientHandler(type);
        }
    }

    private IEnumerable<Type> GetItemsWithSuffix(bool isSingleton, string[] suffixes)
    {
        var types = isSingleton ? _singletonTypes : _transientTypes;

        if (!suffixes.Any())
            return types;

        var list = new List<Type>();

        foreach (var suffix in suffixes)
        {
            list.AddRange(types.Where(x => x.Name.EndsWith(suffix, StringComparison.InvariantCulture)).ToList());
        }

        return list;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class RegisterAsSingletonAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class RegisterAsTransientAttribute : Attribute
{
}