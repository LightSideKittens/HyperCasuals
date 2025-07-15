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
                for (var i = 0; i < blocks.Count; i++)
                {
                    var d = blocks[i];
                    var stageIndex = stages.IndexOf(d.block.sprite) + 1;
                    if (stageIndex == stages.Count)
                    {
                        Grid.Set(d.index, d.block.next);
                    }
                    else
                    {
                        d.block.sprite = stages[stageIndex];
                    }
                    
                    Instantiate(fx, d.block.transform.position, Quaternion.identity);
                }
            }
        }
    }
}