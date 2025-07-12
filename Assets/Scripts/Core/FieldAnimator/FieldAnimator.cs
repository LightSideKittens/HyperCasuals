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
    public partial class FieldAnimator : MonoBehaviour
    {
        [Serializable]
        public class HandlerDict : UniDict<SpriteRenderer, HandlerWrapper>
        {
            
#if UNITY_EDITOR
            public class SpritePreviewAttribute : Attribute
            {
                public int Size;
                public SpritePreviewAttribute(int size = 100) { this.Size = size; }
            }

            public class SpritePreviewAttributeDrawer
                : OdinAttributeDrawer<SpritePreviewAttribute, SpriteRenderer>
            {
                protected override void DrawPropertyLayout(GUIContent label)
                {
                    var sr = this.ValueEntry.SmartValue;
                    if (sr != null && sr.sprite != null)
                    {
                        Texture2D tex = sr.sprite.texture;
                        Rect rect = EditorGUILayout.GetControlRect(false, this.Attribute.Size);
                        ValueEntry.WeakSmartValue = EditorGUI.ObjectField(rect.Split(0, 2), ValueEntry.SmartValue, typeof(SpriteRenderer), true);
                        EditorGUI.DrawTextureTransparent(rect.Split(1, 2), tex, ScaleMode.ScaleToFit);
                    }
                }
            }
            
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
        
        [Serializable]
        public abstract class Handler
        {
            [NonSerialized] public FieldManager fieldManager;
            
            public abstract void Handle();
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

        private void OnDestroyBlocks(SpriteRenderer blockPrefab)
        {
            handlers[blockPrefab].handler.Handle();
        }
    }
}