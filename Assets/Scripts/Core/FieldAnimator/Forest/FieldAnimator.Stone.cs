using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Extensions;
using UnityEngine;

namespace Core
{
    public partial class FieldAnimator
    {
        [Serializable]
        public class Stone : SpecialHandler
        {
            public List<Sprite> stages;
            public ParticleSystem fx;
            private Dictionary<Vector2Int, Sprite> cache = new();
            public override int Priority => 1;

            public override void Handle()
            {
                for (var i = 0; i < blocks.Count; i++)
                {
                    var index = blocks[i].index;
                    var block = Grid.Get(index);
                    if(!block) continue;
                    if (!cache.TryGetValue(index, out var sprite))
                    { 
                        sprite = block.sprite;
                        cache[index] = sprite;
                    }
                    
                    var stageIndex = stages.IndexOf(sprite);
                    if (stageIndex == -1)
                    {
                        continue;
                    }
                    stageIndex++;
                    Action action;
                    if (stageIndex == stages.Count)
                    {
                        cache[index] = null;
                        action = () =>
                        {
                            cache.Remove(index);
                            if (block.next)
                            { 
                                block.next.transform.SetParent(block.transform.parent, true);
                            }
                            Destroy(block.gameObject);
                        };
                        Grid.Set(index, block.next);
                    }
                    else
                    {
                        sprite = stages[stageIndex];
                        action = () =>
                        {
                            if (block) block.sprite = sprite;
                        };
                        cache[index] = sprite;
                    }

                    action += () =>
                    {
                        if (block) Instantiate(fx, block.transform.position, Quaternion.identity);
                    };
                    anim.Add((block, action));
                }
            }
        }
    }
}