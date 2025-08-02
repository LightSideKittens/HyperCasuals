using LSCore;
using LSCore.Attributes;
using SourceGenerators;

[InstanceProxy]
public partial class Themes : SingleScriptableObject<Themes>
{
    [SceneSelector] public string[] _list;
}