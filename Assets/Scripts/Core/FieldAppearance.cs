using System.Collections.Generic;
using LSCore;
using SourceGenerators;
using UnityEngine;

[InstanceProxy]
public partial class FieldAppearance : SingleService<FieldAppearance>
{
    public SpriteRenderer _back;
    public ParticleSystem _shapeAppearFx;
    public SpriteRenderer _selector;
    public List<Block> _blockPrefabs;
    public List<Block> _specialBlockPrefabs;
}