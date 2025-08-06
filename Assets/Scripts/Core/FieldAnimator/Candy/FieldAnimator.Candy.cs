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
        public class Candy : Handler
        {
            public ParticleSystem fx;
            public AnimationCurve anim;
            public float durScale = .3f;

            public override void Handle()
            {
                Sequence seq = DOTween.Sequence();
                
                
                var lines = FieldManager.GetBlockLines(true, true);
                foreach (var data in lines)
                {
                    foreach (var (index, block) in data)
                    {
                        var tr = block.transform;
                        var pos = (index.x + index.y) / 4f * 0.05f;
                        seq.Insert(pos, tr.DOScale(0f, durScale)
                            .SetEase(anim).OnComplete(() =>
                            {
                                var xueta = tr.position;
                                xueta.z = -5f;
                                Instantiate(fx, xueta, fx.transform.rotation);
                                Destroy(block.gameObject);
                            }));
                    }
                }
            }
        }
    }
}