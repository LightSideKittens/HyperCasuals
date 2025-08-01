using LSCore;
using LSCore.Extensions;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class BlockPlaceholder : MonoBehaviour
{
    public FieldAppearance.BlockData data;
    [HideInInspector] public Block block;
    public BlockPlaceholder next;
    private static bool isEdited;
    
    private void Awake()
    {
        InitBlock();
    }

    private void Start()
    {
        if (World.IsPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void InitBlock()
    {
        if(block) return;
        var prefab = data.Block;
        block = Block.Create(prefab);
        block.transform.SetParent(transform);
        block.transform.localPosition = Vector3.zero;
        block.transform.localScale = Vector3.one;
        if (next)
        {
            next.InitBlock();
            block.next = next.block;
        }
        block.sortingOrder = block.sortingOrder;
        if (World.IsPlaying)
        {
            var bonus = GetComponentInChildren<TextMeshPro>();
            if (bonus)
            {
                bonus.transform.SetParent(block.transform, true);
            }
            
            var shape = transform.parent.GetComponent<Shape>();
            if (block.next)
            {
                block.next.transform.SetParent(block.transform, true);
            }
            if (shape)
            { 
                shape.blocks.Add(block);
            }
        }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if(World.IsPlaying) return;
        isEdited = true;
    }
    
    private void Update()
    {
        if (World.IsPlaying) return;
        InitBlock();
        block.gameObject.hideFlags = HideFlags.HideAndDontSave;
        
        if (block != null && isEdited)
        {
            DestroyImmediate(block.gameObject);
            block = null;
        }
    }

    private void LateUpdate()
    {
        isEdited = false;
    }
#endif
}