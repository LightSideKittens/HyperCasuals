using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector] public Block prefab;
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
    public bool IsSpecial => FieldManager.SpecialBlockPrefabs.Contains(prefab);

    public bool ContainsRegular
    {
        get
        {
            if(!IsSpecial) return true;
            if(next is null) return false;
            return next.ContainsRegular;
        }
    }
}