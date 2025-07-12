using DG.Tweening;
using UnityEngine;

namespace Core
{
    public partial class FieldAnimator
    {
        public class Caramel : Handler
        {
            public ParticleSystem fx;
            public AnimationCurve anim;
            public float durScale = .5f;
            public override void Handle()
            {
                Sequence seq = DOTween.Sequence();
                

                if (fieldManager.uniqueSuicidesData.Count == 0) return;
                
                foreach (var data in fieldManager.uniqueSuicidesData)
                {
                    foreach (var (index, block) in data)
                    {
                        var tr = block.transform;
                        var pos = (index.x + index.y) / 2f * 0.05f;
                        seq.Insert(pos, tr.DOScale(0f, durScale)
                            .SetEase(anim).OnComplete(() =>
                            {
                                Instantiate(fx, tr.position, Quaternion.identity);
                                Destroy(block.gameObject);
                            }));
                    }
                }
            }
        }
    }
}