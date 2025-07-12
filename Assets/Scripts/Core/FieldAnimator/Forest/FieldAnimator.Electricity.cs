using System;
using UnityEngine;
using DG.Tweening;


namespace Core
{
    public partial class FieldAnimator
    {
        [Serializable]
        public class Electricity : Handler
        {
            public ParticleSystem fx;
            public AnimationCurve doScaleFx;

            public override void Handle()
            {
                Sequence seq = DOTween.Sequence();

                if (fieldManager.uniqueSuicidesData.Count == 0) return;
                
                foreach (var data in fieldManager.uniqueSuicidesData)
                {
                    var fxInstance = Instantiate(fx, data[0].block.transform.position, Quaternion.identity);
                    fxInstance.transform.position = data[0].block.transform.position;

                    foreach (var (index, block) in data)
                    {
                        var tr = block.transform;
                        var pos = (index.x + index.y) / 2f * 0.05f;
                        seq.Insert(pos, fxInstance.transform.DOMove(data[^1].block.transform.position, 0.4f)
                            .SetEase(Ease.InCubic));
                        seq.Insert(pos, fxInstance.transform.DOScale(1f, 0.4f)
                            .SetEase(doScaleFx));
                        seq.Insert(pos  + 0.1f, block.DOFade(0, 0.5f)
                            .SetEase(Ease.InCubic).OnComplete(() =>
                            {
                                Destroy(block.gameObject);
                            }));

                    }
                }
            }
        }
    }
}