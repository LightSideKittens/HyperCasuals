using System;
using LSCore;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Block : MonoBehaviour
{
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

    public bool ContainsRegular
    {
        get
        {
            if(!IsSpecial) return true;
            if(next is null) return false;
            return next.ContainsRegular;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(World.IsPlaying) return;
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        GameObject root;
        try
        {
            root = stage != null ? stage.prefabContentsRoot : null;
        }
        catch (Exception)
        {
            return;
        }
        
        if (!AssetDatabase.Contains(gameObject) && root != gameObject)
        {
            var newPrefab = PrefabUtility.GetCorrespondingObjectFromSource(this);
            if (prefab != newPrefab)
            {
                prefab = newPrefab;
                EditorUtility.SetDirty(this);
            }
        }
    }
#endif
    
    public static Block Create(Block prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        var block = Instantiate(prefab, pos, rot, parent);
        block.prefab = prefab;
        return block;
    }
    
    public static Block Create(Block prefab) => Create(prefab, Vector3.zero, Quaternion.identity, null);
}