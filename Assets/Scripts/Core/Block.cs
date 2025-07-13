using UnityEngine;

public class Block : MonoBehaviour
{
    public SpriteRenderer render;

    public Sprite sprite
    {
        get => render.sprite;
        set => render.sprite = value;
    }

    public Color color
    {
        get => render.color;
        set => render.color = value;
    }

    public int sortingOrder
    {
        get => render.sortingOrder;
        set => render.sortingOrder = value;
    }
    
    public Block next;
    
    public void SetNext(Block prefab)
    {
        var next = Instantiate(prefab, transform.parent);
        next.sortingOrder = sortingOrder - 1;
        this.next = next;
    }
}