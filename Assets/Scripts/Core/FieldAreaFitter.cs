using System;
using LSCore;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace Core
{
    [ExecuteAlways]
    public class FieldAreaFitter : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_EDITOR
            if(World.IsEditMode) return;
#endif
            FieldManager.Starting += OnStarting;
        }

        private void OnStarting()
        {
            FieldManager.Starting -= OnStarting;
            Fit();
        }

        private void OnDestroy()
        {
            FieldManager.Starting -= OnStarting;
        }

#if UNITY_EDITOR
        public void Update()
        {
            if(World.IsPlaying) return;
            Fit();
        }
#endif

        private void Fit()
        {
            var rectTransform = GetComponent<RectTransform>();
            var worldRect = rectTransform.GetWorldRect();
            var area = FieldAppearance.Area.rect;

            var field = FieldAppearance.Field;
            field.transform.position = worldRect.center;
            field.transform.localScale = worldRect.size / area.size;
        }
    }
}