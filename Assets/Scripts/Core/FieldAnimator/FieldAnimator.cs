using System;
using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.DataStructs;
using LSCore.Extensions;
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
            [NonSerialized] public FieldAnimator animator;
            
            public Block[,] Grid => fieldManager.grid;
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
                handler.handler.animator = this;
            }
        }

        private void OnDestroyBlocks(Block block)
        {
            var specialBlockPrefabs = fieldManager.GetSpecialBlocks(fieldManager.UniqueSuicidesData);

            foreach (var (prefab, specialBlocks) in specialBlockPrefabs)
            {
                for (int i = 0; i < specialBlocks.Count; i++)
                {
                    var data = specialBlocks[i];
                    fieldManager.RemoveData(data);
                }
                
                var h = handlers[prefab].handler as SpecialHandler;
                h!.blocks = specialBlocks;
                h.Handle();
            }
            
            for (int j = 0; j < fieldManager.uniqueSuicidesData.Count; j++)
            {
                var list = fieldManager.uniqueSuicidesData[j];
                for (int i = 0; i < list.Count; i++)
                {
                    fieldManager.grid.Set(list[i].index, null);
                }
            }

            handlers[block].handler.Handle();
        }
    }
}