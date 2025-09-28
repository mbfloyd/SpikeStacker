using System;

public class SceneDIContainer
{
    private readonly DIContainer _parent;
    private readonly DIContainer _local;

    public SceneDIContainer(DIContainer parent = null)
    {
        _parent = parent ?? DIContainer.Instance;
        _local = new DIContainer(); // fresh local scope
    }

    public void Register<TInterface, TImplementation>(Lifetime lifetime = Lifetime.Singleton)
        where TImplementation : TInterface, new()
        => _local.Register<TInterface, TImplementation>(lifetime);

    public void Register<TInterface>(Func<TInterface> factory, Lifetime lifetime = Lifetime.Singleton)
        => _local.Register(factory, lifetime);

    public object Resolve(Type type, Type key = null)
    {
        try { return _local.Resolve(type, key); }
        catch { return _parent.Resolve(type, key); }
    }

    public T Resolve<T>() => (T)Resolve(typeof(T));
    public T Resolve<T, TKey>() => (T)Resolve(typeof(T), typeof(TKey));

    public void InjectFields(object target)
    {
        try { _local.InjectFields(target); }
        catch { _parent.InjectFields(target); }
    }
}
