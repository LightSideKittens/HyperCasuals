using LSCore;
using SourceGenerators;

[InstanceProxy]
public abstract partial class BaseInitializer : SingleService<BaseInitializer>
{
    protected abstract void _Initialize();
}