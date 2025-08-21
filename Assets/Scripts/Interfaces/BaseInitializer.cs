using System;
using LSCore;
using SourceGenerators;

[InstanceProxy]
public abstract partial class BaseInitializer : SingleService<BaseInitializer>
{
    private static bool initialized = false;
#if UNITY_EDITOR
    static BaseInitializer()
    {
        World.Destroyed += () => initialized = false;
    }
#endif

    private void _Initialize(Action onInitialized)
    {
        if (initialized)
        {
            onInitialized?.Invoke();
            return;
        }
        
        initialized = true;
        OnInitialize(onInitialized);
    }
    
    protected abstract void OnInitialize(Action onInitialized);
}