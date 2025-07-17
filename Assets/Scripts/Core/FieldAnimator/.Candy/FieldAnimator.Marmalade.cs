using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Core
{
    public partial class FieldAnimator
    {
        public class Marmalade : Handler
        {
            [SerializeField] public float probabilityEffect = 0.5f;
            public AnimationCurve animationCurve;
            public ParticleSystem fx;
            public HashSet<Vector2Int> blocksToExplode = new();

            public override void Handle()
            {
                foreach (var (index, _) in fieldManager.UniqueSuicidesData)
                {
                    if (Random.value < probabilityEffect)
                    {
                        blocksToExplode.Add(index);
                    }
                }

                foreach (var (index, block) in fieldManager.UniqueSuicidesData)
                {
                    var seq = DOTween.Sequence();
                    var tr = block.transform;

                    seq.Append(tr.DOScale(0, .5f).SetEase(animationCurve));
                    seq.OnComplete(() =>
                    {
                        if (blocksToExplode.Contains(index))
                        {

                            Instantiate(fx, block.transform.position, fx.transform.rotation);
                        }

                        Destroy(block.gameObject);
                    });
                }
            }
        }
    }
}