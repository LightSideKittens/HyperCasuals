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
            
            public override void Handle()
            {
                for (var i = 0; i < indices.Count; i++)
                {
                    var index = indices[i];
                    var block = Grid.Get(index);
                    if(!block) continue;
                    if(!stages.Contains(block.sprite)) continue;

                    var stageIndex = stages.IndexOf(block.sprite);
                    stageIndex++;
                    
                    if (stageIndex == stages.Count)
                    {
                        block.sprite = null;
                        block.next.transform.SetParent(block.transform.parent, true);
                        Destroy(block.gameObject);
                        Grid.Set(index, block.next);
                    }
                    else
                    {
                        block.sprite = stages[stageIndex];
                    }
                    
                    Instantiate(fx, block.transform.position, Quaternion.identity);
                }
            }
        }
    }
}