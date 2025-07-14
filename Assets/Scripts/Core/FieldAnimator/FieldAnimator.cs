using System;
using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.DataStructs;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace Core
{
#if UNITY_EDITOR
    public class SpritePreviewAttribute : Attribute
    {
        public int Size;
        public SpritePreviewAttribute(int size = 100) { this.Size = size; }
    }
    
    public class SpritePreviewAttributeDrawer
        : OdinAttributeDrawer<SpritePreviewAttribute, Block>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var sr = this.ValueEntry.SmartValue;
            if (sr != null && sr.sprite != null)
            {
                Texture2D tex = sr.sprite.texture;
                Rect rect = EditorGUILayout.GetControlRect(false, this.Attribute.Size);
                ValueEntry.WeakSmartValue = EditorGUI.ObjectField(rect.Split(0, 2), ValueEntry.SmartValue, typeof(Block), true);
                EditorGUI.DrawTextureTransparent(rect.Split(1, 2), tex, ScaleMode.ScaleToFit);
            }
            else
            {
                CallNextDrawer(label);
            }
        }
    }
#endif
    
    [Serializable]
    public abstract class BaseBlockDict<T> : UniDict<Block, T>
    {
            
#if UNITY_EDITOR
        protected override void OnKeyProcessAttributes(List<Attribute> attributes)
        {
            attributes.Add(new SpritePreviewAttribute());
            attributes.Add(new PropertySpaceAttribute() {SpaceAfter = 20});
        }

        protected override void OnValueProcessAttributes(List<Attribute> attributes)
        {
            attributes.Add(new UnwrapAttribute());
        }
#endif
    }
    
    public partial class FieldAnimator : MonoBehaviour
    {
        [Serializable]
        public class HandlerDict : BaseBlockDict<HandlerWrapper> { }
        
        [Serializable]
        public abstract class Handler
        {
            [NonSerialized] public FieldManager fieldManager;
            
            public abstract void Handle();
        }
        
        [Serializable]
        public abstract class SpecialHandler : Handler
        {
            [NonSerialized] public List<(Vector2Int index, Block block)> blocks;
        }

        [Serializable]
        public class HandlerWrapper
        {
            [SerializeReference] public Handler handler;
        }
        
        public FieldManager fieldManager;
        
        public HandlerDict handlers = new();

        private void Awake()
        {
            fieldManager.BlocksDestroying += OnDestroyBlocks;

            foreach (var handler in handlers.Values)
            {
                handler.handler.fieldManager = fieldManager;
            }
        }

        private void OnDestroyBlocks(Block block)
        {
            var specialBlockPrefabs = new HashSet<Block>();
            var specialBlocks = new List<(Vector2Int index, Block block)>();
            
            foreach (var valueTuple in fieldManager.UniqueSuicidesData)
            {
                var prefab = valueTuple.block.prefab;
                if (FieldManager.SpecialBlockPrefabs.Contains(prefab))
                {
                    specialBlockPrefabs.Add(prefab);
                    specialBlocks.Add(valueTuple);
                }
            }

            foreach (var data in specialBlocks)
            {
                for (int i = 0; i < fieldManager.suicidesData.Count; i++)
                {
                    var list = fieldManager.suicidesData[i]; 
                    list.Remove(data);
                }
                
                for (int i = 0; i < fieldManager.uniqueSuicidesData.Count; i++)
                {
                    var list = fieldManager.uniqueSuicidesData[i]; 
                    list.Remove(data);
                }
            }

            foreach (var specialBlockPrefab in specialBlockPrefabs)
            {
                var h = handlers[specialBlockPrefab].handler as SpecialHandler;
                h!.blocks = specialBlocks;
                h.Handle();
            }

            handlers[block].handler.Handle();
        }
    }
}