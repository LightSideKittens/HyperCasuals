using LSCore;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class BlockPlaceholder : MonoBehaviour
{
    public FieldAppearance.BlockData data;
    [HideInInspector] public Block block;
    public BlockPlaceholder next;
    private static bool isEdited;
    private SpriteRenderer dummySpriteRenderer;
    
    protected virtual void Awake()
    {
        block = null;
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
#if UNITY_EDITOR
        if (World.IsEditMode)
        { 
            block.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
#endif
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
            var bonus = GetComponentInChildren<BonusBlock>();
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
        EditorApplication.update += OnUpdate;
        isEdited = true;

        void OnUpdate()
        {
            EditorApplication.update -= OnUpdate;
            if (dummySpriteRenderer == null)
            {
                dummySpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                dummySpriteRenderer.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                dummySpriteRenderer.sprite = data.Block.sprite;
                dummySpriteRenderer.color = Color.clear;
            }
        }
    }
    
    private void Update()
    {
        if (World.IsPlaying) return;
        InitBlock();
        
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