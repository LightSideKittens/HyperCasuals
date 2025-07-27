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
                var uniqueBlocks = fieldManager.GetBlockLines(true, true);
                var blocks = fieldManager.linesIndices;
                
                for (var i = 0; i < uniqueBlocks.Count; i++)
                {
                    var uniqueData = uniqueBlocks[i];
                    var data = blocks[i];
                    var dir = ((Vector2)(data[0] - data[1])).normalized;
                    var fxPos = fieldManager._ToPos(data[0]) + (dir * ofsetFxPos);
                    
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