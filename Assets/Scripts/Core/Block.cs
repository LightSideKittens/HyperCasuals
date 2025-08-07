using System;
using System.Collections.Generic;
using Core;
using LSCore;
using Sirenix.OdinInspector;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Id(typeof(BlockIdGroup))] public Id id;
    [ReadOnly] public Block prefab;
    public SpriteRenderer render;
    [NonSerialized] public int defaultSortingOrder;
    
    public Sprite sprite
    {
        get => render.sprite;
        set => render.sprite = value;
    }

    public Color color
    {
        get => render.color;
        set
        {
            render.color = value;
            if(next) next.color = value;
        }
    }

    public int sortingOrder
    {
        get => render.sortingOrder;
        set
        {
            render.sortingOrder = value;
            if(next) next.sortingOrder = value - 1;
        }
    }

    public Block next;
    public bool IsSpecial => FieldAppearance.SpecialBlockPrefabs.Contains(prefab);

    public Block GetRegular()
    {
        if(!IsSpecial) return this;
        return next?.GetRegular();
    }
    
    public IEnumerable<Block> AllBlocks
    {
        get
        {
            var block = this;
            while (block.next is not null)
            {
                yield return block;
                block = block.next;
            }
            yield return block;
        }
    }
    
    public static Block Create(Block prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        var block = Instantiate(prefab, pos, rot, parent);
        block.prefab = prefab;
        return block;
    }
    
    public static Block Create(Block prefab) => Create(prefab, Vector3.zero, prefab.transform.localRotation, null);
}