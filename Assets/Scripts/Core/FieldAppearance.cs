using System;
using System.Collections.Generic;
using LSCore;
using LSCore.Attributes;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using SourceGenerators;
using UnityEngine;

[InstanceProxy]
public partial class FieldAppearance : SingleService<FieldAppearance>
{
    [Serializable]
    [Unwrap]
    public struct BlockData
    { 
        [ValueDropdown("Blocks")] public int index;
        public bool isSpecial;
        public Block Block => isSpecial ? SpecialBlockPrefabs.GetCyclic(index) : BlockPrefabs.GetCyclic(index);

        private IEnumerable<ValueDropdownItem<int>> Blocks => GetBlocks(isSpecial);
    }
    
    public SpriteRenderer _back;
    public ParticleSystem _shapeAppearFx;
    public SpriteRenderer _selector;
    public List<Block> _blockPrefabs;
    public List<Block> _specialBlockPrefabs;

    public IEnumerable<ValueDropdownItem<int>> _GetBlocks(bool isSpecial)
    {
        var list = isSpecial ? _specialBlockPrefabs : _blockPrefabs;
        for (int i = 0; i < list.Count; i++)
        {
            var element = list[i];
            yield return new ValueDropdownItem<int>(element.name, i);
        }
    }
}