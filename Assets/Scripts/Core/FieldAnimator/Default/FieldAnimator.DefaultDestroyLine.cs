using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace Core
{
    public partial class FieldAnimator
    {
        [Serializable]
        public class DefaultDestroyLine : Handler
        {
            public override void Handle()
            {
                Sequence seq = DOTween.Sequence();
                var lines = FieldManager.GetBlockLines(true, true);
                foreach (var data in lines)
                {
                    foreach (var (index, block) in data)
                    {
                        var pos = (index.x + index.y) / 4f * 0.05f;
                        var fadeAnim = block.render.DOFade(0, 0.3f)
                            .SetEase(Ease.InCubic)
                            .OnComplete(() => { Destroy(block.gameObject); });
                        var scaleAnim = block.transform.DOScale(1.5f.ToVector3(), 0.1f).SetLoops(2, LoopType.Yoyo);
                        seq.Insert(pos, fadeAnim);
                        seq.Insert(pos, scaleAnim);
                    }
                }
            }
        }
    }
}