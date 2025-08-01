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
        public class DefaultDestroyLine : SpecialHandler
        {
            public ParticleSystem fx;
            public AnimationCurve doScaleFx;

            public override void Handle()
            {
                Sequence seq = DOTween.Sequence();
                var lines = FieldManager.GetBlockLines(true, true);
                foreach (var data in lines)
                {

                    foreach (var (index, block) in data)
                    {
                        var tr = block.transform;
                        var pos = (index.x + index.y) / 2f * 0.05f;
                        seq.Insert(pos + 0.1f, block.render.DOFade(0, 0.5f)
                            .SetEase(Ease.InCubic).OnComplete(() => { Destroy(block.gameObject); }));
                    }
                }
            }
        }
    }
}