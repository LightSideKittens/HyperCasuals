#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using LSCore;
using LSCore.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core
{
    [Serializable]
    public class ScenesModifier : AssetsModifier.BaseGameObjectsModifier
    {

        protected override bool OnEnter(List<GameObject> roots, out bool needBrake)
        {
            needBrake = false;
            
            /*foreach (var root in roots)
            {
                var gates = root.transform.Find("Gates");
                if (gates != null)
                {
                    var cups = root.GetComponentsInChildren<Cup>();
                    foreach (var cup in cups)
                    {
                        var p = cup.transform.localPosition;
                        p.y = 0;
                        cup.transform.localPosition = p;
                        EditorUtility.SetDirty(cup);
                    }
                    needBrake = true;
                    return true;
                }
            }*/
            
            return false;
        }

        public GameObject horizontalPrefab;
        public GameObject verticalPrefab;

        protected override bool Modify(GameObject go, out bool needBreak)
        {
            needBreak = false;
            
            /*if (go.TryGetComponent<Shape>(out var shape))
            {
                var path = AssetDatabase.GetAssetPath(go);
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var root  = scope.prefabContentsRoot;
                    shape = root.GetComponent<Shape>();
                    shape.horizontalArrows = (GameObject)PrefabUtility.InstantiatePrefab(horizontalPrefab, root.transform);
                    shape.verticalArrows = (GameObject)PrefabUtility.InstantiatePrefab(verticalPrefab, root.transform);
                    var sprites = shape.horizontalArrows.GetComponentsInChildren<SpriteRenderer>();

                    foreach (var sprite in sprites)
                    {
                        var s = sprite.size;
                        var p = sprite.transform.localPosition;
                        p.x *= shape.size.x;
                        s.x *= shape.size.x;
                        sprite.transform.localPosition = p;
                        sprite.size = s;
                    }
                    
                    sprites = shape.verticalArrows.GetComponentsInChildren<SpriteRenderer>();

                    foreach (var sprite in sprites)
                    {
                        var s = sprite.size;
                        var p = sprite.transform.localPosition;
                        p.x *= shape.size.y;
                        s.x *= shape.size.y;
                        sprite.transform.localPosition = p;
                        sprite.size = s;
                    }
                }
                return true;
            }*/

            
            return false;
            
            /*needBreak = false;
            
            if (go.TryGetComponent<CoreWorld>(out var world))
            {
                PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                return true;
            }
            
            var cups = cupsList.ToDictionary(x => x.data[0].id);
            var bubbles = bubblesList.ToDictionary(x => x.id);
            
            bool modified = false;
            
            if (go.TryGetComponent<Cup>(out var cup))
            {
                var id = cup.data[0].id;
                var replacement = cups[id];
                PrefabUtility.ReplacePrefabAssetOfPrefabInstance(go, replacement.gameObject, new PrefabReplacingSettings()
                {
                    prefabOverridesOptions = PrefabOverridesOptions.ClearAllNonDefaultOverrides
                }, InteractionMode.AutomatedAction);
                modified = true;
            }
            else if (go.TryGetComponent<Bubble>(out var bubble))
            {
                var id = bubble.id;
                var replacement = bubbles[id];
                PrefabUtility.ReplacePrefabAssetOfPrefabInstance(go, replacement.gameObject, new PrefabReplacingSettings()
                {
                    prefabOverridesOptions = PrefabOverridesOptions.ClearAllNonDefaultOverrides
                }, InteractionMode.AutomatedAction);
                modified = true;
            }

            return modified;*/
        }
    }
}
#endif