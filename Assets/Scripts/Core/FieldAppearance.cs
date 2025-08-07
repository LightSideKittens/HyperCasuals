using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BlockData
    { 
        [ValueDropdown("Blocks")] public Id id;
        public bool isSpecial;
        public Block Block
        {
            get
            {
                if (id == null)
                {
                    return isSpecial ? SpecialBlockPrefabs[0] : BlockPrefabs[0];
                }
                
                return isSpecial
                    ? SpecialBlockPrefabs.FirstOrDefault(x => x.id == id)
                    : BlockPrefabs.FirstOrDefault(x => x.id == id);
            }
        }

        private IEnumerable<ValueDropdownItem<Id>> Blocks => GetBlocks(isSpecial);
    }
    
    public SpriteRenderer _back;
    public ParticleSystem _shapeAppearFx;
    public SpriteRenderer _selector;
    public List<Block> _blockPrefabs;
    public List<Block> _specialBlockPrefabs;

    public IEnumerable<ValueDropdownItem<Id>> _GetBlocks(bool isSpecial)
    {
        var list = isSpecial ? _specialBlockPrefabs : _blockPrefabs;
        for (int i = 0; i < list.Count; i++)
        {
            var element = list[i];
            yield return new ValueDropdownItem<Id>(element.name, element.id);
        }
    }
}