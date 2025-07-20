using System;
using System.Collections.Generic;
using System.Linq;
using LSCore;
using LSCore.Attributes;
using LSCore.DataStructs;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SourceGenerators;
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
    
    [InstanceProxy]
    public partial class FieldAnimator : SingleService<FieldAnimator>
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
            public List<(Block block, Action action)> anim = new();
            public virtual int Priority => 0;
            public void Animate()
            {
                foreach (var data in anim)
                {
                    data.action?.Invoke();
                }

                anim.Clear();
            }
        }

        [Serializable]
        public class HandlerWrapper
        {
            [SerializeReference] public Handler handler;
        }
        
        public FieldManager fieldManager;
        
        public HandlerDict _handlers = new();

        protected override void Init()
        {
            base.Init();
            fieldManager.BlocksDestroying += OnDestroyBlocks;

            foreach (var handler in _handlers.Values)
            {
                handler.handler.fieldManager = fieldManager;
                handler.handler.animator = this;
            }
        }

        private void _Simulate()
        {
            var blocks = fieldManager.GetBlocks(true, true);
            for (int j = 0; j < blocks.Count; j++)
            {
                var d = blocks[j];
                fieldManager.grid.Set(d.index, null);
            }
            
            currentSpecialBlocks = fieldManager.GetSpecialBlocks(
                fieldManager.GetBlocks(false, false)
                    .Select(x => x.index));

            foreach (var (prefab, specialBlocks) in currentSpecialBlocks)
            {
                var h = _handlers[prefab].handler as SpecialHandler;
                h!.blocks = specialBlocks;
                h.Handle();
                h.anim.Clear();
            }
            currentSpecialBlocks.Clear();
        }

        public Dictionary<Block, List<(Vector2Int index, Block block)>> currentSpecialBlocks = new();

        public bool ContainsInSpecialBlocks(Block block)
        {
            return currentSpecialBlocks.Any(x => x.Value.Any(y => y.block == block));
        }
        
        private void OnDestroyBlocks(Block block)
        {
            _handlers[block].handler.Handle();

            var blocks = fieldManager.GetBlocks(true, true);
            for (int j = 0; j < blocks.Count; j++)
            {
                var d = blocks[j];
                fieldManager.grid.Set(d.index, null);
            }
            
            currentSpecialBlocks = fieldManager.GetSpecialBlocks(
                fieldManager.GetBlocks(false, false)
                    .Select(x => x.index));

            foreach (var (prefab, specialBlocks) in currentSpecialBlocks.OrderByDescending(x =>
                     {
                         var h = _handlers[x.Key].handler as SpecialHandler;
                         return h!.Priority;
                     }))
            {
                var h = _handlers[prefab].handler as SpecialHandler;
                h!.blocks = specialBlocks;
                h.anim.Clear();
                h.Handle();
                h.Animate();
            }
            currentSpecialBlocks.Clear();

        }
    }
}