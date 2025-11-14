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
        
        public Block Block => GetBlock(Instance);
        public Block GetBlock(FieldAppearance fieldAppearance)
        {
            if (id == null)
            {
                return isSpecial ? fieldAppearance._specialBlockPrefabs[0] : fieldAppearance._blockPrefabs[0];
            }
                
            return isSpecial
                ? fieldAppearance._specialBlockPrefabs.FirstOrDefault(x => x.id == id)
                : fieldAppearance._blockPrefabs.FirstOrDefault(x => x.id == id);
        }

        private IEnumerable<ValueDropdownItem<Id>> Blocks
        {
            get
            {
                if(IsNull) return Levels.FieldAppearance[GameSave.Theme]._GetBlocks(isSpecial);
                return GetBlocks(isSpecial);
            }
        }
    }
    
    public RectTransform _area;
    public Transform _field;
    public SpriteRenderer _back;
    public SpriteRenderer _redBack;
    public ParticleSystem _shapeAppearFx;
    public SpriteRenderer _selector;
    public List<Block> _blockPrefabs;
    public List<Block> _specialBlockPrefabs;
    public static float FieldScale => Instance._field.localScale.x;
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