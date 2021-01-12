using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

class DependencyManager
{
    Dictionary<Type, object> _typeMapper = new Dictionary<Type, object>();

    /// <summary>
    /// Add a mapping between a type and an instance. Normally this is a mapping
    /// between an interface type - e.g. IXmlSplitter and the instance of a class which implements the type.
    /// But it can be a mapping between a normal class type and an instance.
    /// </summary>
    /// <typeparam name="InterfaceType">a type</typeparam>
    /// <param name="instance">an instance of the type or an instance of a type which implements the interface</param>
    public void AddMapping<InterfaceType>(InterfaceType instance)
    {
        _typeMapper.Add(typeof(InterfaceType), instance);
    }

    public T GetMapping<T>() where T:class
    {
        if (!_typeMapper.TryGetValue(typeof(T), out var instance))
        {
            return null;
        }

        return instance as T;
    }

    // we need to match ALL the parameters of a constructor otherwise
    // we cannot run it. We'll look for each parameter of the constructor in the _typeMapper Dictionary.
    // We take constructors which match the extraConstructorParams at the end.
    public T CreateInstance<T>(params object[] extraConstructorParams)
    {
        var ctors = typeof(T).GetConstructors();

        var matching = new List<ConstructorInfo>();
        foreach (var ctor in ctors)
        {
            var cparams = ctor.GetParameters().Select(x => x.GetType()).ToArray();
            bool match = true;

            if (cparams.Length < extraConstructorParams.Length)
            {
                // not enough ctor params to match extra params:
                continue;
            }

            for (int i = extraConstructorParams.Length - 1; i < 0; i--)
            {
                if (extraConstructorParams[i].GetType() != cparams[i].GetType())
                {
                    match = false;
                    break;
                }
            }

            if (!match)
            {
                continue;
            }

            for (int i = 0; i < cparams.Length - extraConstructorParams.Length; i++)
            {
                var type = cparams[i];
                if (!_typeMapper.TryGetValue(type, out var instance))
                {
                    match = false;
                }
            }

            if (!match)
            {
                continue;
            }

            matching.Add(ctor);
        }

        if (matching.Count == 0)
        {
            throw new Exception($"Cannot create instance of type as there are no matching constructors.");
        }
        else if (matching.Count != 1)
        {
            throw new Exception($"Ambiguous: more than one constructor matches.");
        }

        return (T)matching.First().Invoke(extraConstructorParams);
    }
}