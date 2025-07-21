using System;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class RectToFieldFitter : DoIt
    {
        public RectTransform rect;
        
        public override void Do()
        {
            var fieldRect = FieldManager.FieldRect;
            var canvas = rect.GetComponentInParent<Canvas>();
            rect.FitToWorldRect(fieldRect, canvas.worldCamera, canvas);
        }
    }
}