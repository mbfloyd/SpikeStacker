using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


public enum Lifetime { Singleton, Transient }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
public class InjectAttribute : Attribute
{
    public Type KeyType { get; set; } = null;
}

internal class ServiceDescriptor
{
    public Func<object> Factory;
    public Lifetime Lifetime;
    private object _cachedInstance;

    public object GetInstance()
    {
        if (Lifetime == Lifetime.Singleton)
            return _cachedInstance ??= Factory();
        return Factory();
    }
}

public class DIContainer
{
    private static DIContainer _instance;
    public static DIContainer Instance => _instance ??= new DIContainer();

    private readonly Dictionary<(Type, Type), ServiceDescriptor> _services = new();

    #region Registration

    public void Register<TInterface, TImplementation>(Lifetime lifetime = Lifetime.Singleton)
        where TImplementation : TInterface, new()
    {
        _services[(typeof(TInterface), null)] = new ServiceDescriptor
        {
            Lifetime = lifetime,
            Factory = () => CreateInstance(typeof(TImplementation))
        };
    }

    public void Register<TInterface, TImplementation, TKey>(Lifetime lifetime = Lifetime.Singleton)
        where TImplementation : TInterface, new()
    {
        _services[(typeof(TInterface), typeof(TKey))] = new ServiceDescriptor
        {
            Lifetime = lifetime,
            Factory = () => CreateInstance(typeof(TImplementation))
        };
    }

    public void Register<TInterface>(Func<TInterface> factory, Lifetime lifetime = Lifetime.Singleton)
    {
        _services[(typeof(TInterface), null)] = new ServiceDescriptor
        {
            Lifetime = lifetime,
            Factory = () => factory()
        };
    }

    public void Register<TInterface, TKey>(Func<TInterface> factory, Lifetime lifetime = Lifetime.Singleton)
    {
        _services[(typeof(TInterface), typeof(TKey))] = new ServiceDescriptor
        {
            Lifetime = lifetime,
            Factory = () => factory()
        };
    }

    #endregion

    #region Resolution

    public TInterface Resolve<TInterface>() => (TInterface)Resolve(typeof(TInterface), null);
    public TInterface Resolve<TInterface, TKey>() => (TInterface)Resolve(typeof(TInterface), typeof(TKey));

    public object Resolve(Type type, Type key = null)
    {
        if (_services.TryGetValue((type, key), out var descriptor))
            return descriptor.GetInstance();
        if (_services.TryGetValue((type, null), out var defaultDescriptor))
            return defaultDescriptor.GetInstance();
        if (type.IsClass)
            return CreateInstance(type);

        throw new Exception($"Cannot resolve type {type} (key {key?.Name ?? "default"})");
    }

    public IEnumerable<TInterface> ResolveAll<TInterface>()
    {
        var type = typeof(TInterface);
        return _services
            .Where(kvp => kvp.Key.Item1 == type)
            .Select(kvp => (TInterface)kvp.Value.GetInstance())
            .ToList();
    }

    #endregion

    #region Instance Creation

    private object CreateInstance(Type type)
    {
        if (typeof(MonoBehaviour).IsAssignableFrom(type))
            throw new InvalidOperationException($"Cannot create MonoBehaviour {type}. Use AddComponent instead.");
        if (typeof(ScriptableObject).IsAssignableFrom(type))
            return ScriptableObject.CreateInstance(type);

        var ctor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
        if (ctor == null) throw new InvalidOperationException($"No public constructor found for {type}");

        var parameters = ctor.GetParameters().Select(ResolveParameter).ToArray();
        var instance = ctor.Invoke(parameters);

        InjectFields(instance);

        return instance;
    }

    private object ResolveParameter(ParameterInfo parameter)
    {
        var paramType = parameter.ParameterType;
        var injectAttr = parameter.GetCustomAttribute<InjectAttribute>();

        if (paramType.IsGenericType &&
            (paramType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
             paramType.GetGenericTypeDefinition() == typeof(List<>)))
        {
            var elementType = paramType.GetGenericArguments()[0];
            var all = ResolveAll(elementType);
            if (paramType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var toListMethod = typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod(elementType);
                return toListMethod.Invoke(null, new object[] { all });
            }
            return all;
        }

        return Resolve(paramType, injectAttr?.KeyType);
    }

    private IEnumerable<object> ResolveAll(Type type)
    {
        return _services.Where(kvp => kvp.Key.Item1 == type)
                        .Select(kvp => kvp.Value.GetInstance());
    }

    #endregion

    #region Field Injection

    public void InjectFields(object target)
    {
        var fields = target.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var injectAttr = field.GetCustomAttribute<InjectAttribute>();
            if (injectAttr == null) continue;

            var fieldType = field.FieldType;

            if (fieldType.IsGenericType &&
                (fieldType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 fieldType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                var elementType = fieldType.GetGenericArguments()[0];
                var all = ResolveAll(elementType);
                object value = fieldType.GetGenericTypeDefinition() == typeof(List<>)
                    ? typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod(elementType).Invoke(null, new object[] { all })
                    : all;

                field.SetValue(target, value);
            }
            else
            {
                var dependency = Resolve(fieldType, injectAttr.KeyType);
                field.SetValue(target, dependency);
            }
        }
    }

    #endregion
}
