using System;
using DG.Tweening;
using UnityEngine;


namespace Core
{
    public partial class FieldAnimator
    {
        [Serializable]
        public class Nature : Handler
        {
            public ParticleSystem fx;

            [SerializeField] public float ofsetFxPos = 1;
            public float fxScaleSer;
            public override void Handle()
            {
                Sequence seq = DOTween.Sequence();
                for (var i = 0; i < fieldManager.uniqueSuicidesData.Count; i++)
                {
                    var uniqueData = fieldManager.uniqueSuicidesData[i];
                    var data = fieldManager.suicidesData[i];
                    var dir = (data[0].block.transform.position - data[1].block.transform.position).normalized;
                    var fxPos = data[0].block.transform.position + (dir * ofsetFxPos);
                    
                    var fxScale = Vector3.one * fxScaleSer;
                    fx.transform.localScale = fxScale;
                    Instantiate(fx, fxPos, Quaternion.identity);

                    for (var j = 0; j < uniqueData.Count; j++)
                    {
                        var (index, block) = uniqueData[j];
                        var tr = block.transform;
                        var pos = (index.x + index.y) / 2f * 0.05f;
                        seq.Insert(pos, tr
                            .DOScale(Vector3.zero, 0.7f)
                            .SetEase(Ease.InQuart));
                        
                        seq.Insert(pos, tr
                            .DOMove(fxPos, .7f)
                            .SetEase(Ease.InQuart)
                            .OnComplete(() =>
                            {
                                Destroy(block.gameObject);
                            }));
                    }
                }
            }
        }
    }
}