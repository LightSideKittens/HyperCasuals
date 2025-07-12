using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public partial class FieldAnimator
    {
        [Serializable]
        public class BlockHole : Handler
        {
            public CanvasGroup fx;

            public override void Handle()
            {
                Sequence seq = DOTween.Sequence();

                AnimFX();
                
                var i = 0;
                
                foreach (var (index, block) in fieldManager.UniqueSuicidesData)
                {
                    var duration = Random.Range(0.5f,0.8f);
                    var rotation = Random.Range(270,359);
                    
                    var tr = block.transform;
                    var empty = new GameObject("Empty").transform;
                    empty.position = fx.transform.position;
                    tr.SetParent(empty, true);
                    seq.Insert(i * 0.02f, empty.DORotate(new Vector3(0, 0, rotation), duration).SetEase(Ease.InCirc));   
                    seq.Insert(i * 0.02f,tr.DOScale(.2f, duration).SetEase(Ease.InCirc));
                    seq.Insert(i * 0.02f,block.DOFade(0f, duration).SetEase(Ease.InCirc));
                    seq.Insert(i * 0.02f,tr.DOLocalMove(Vector3.zero, duration).SetEase(Ease.InCirc));
                    seq.Insert(i * 0.02f,tr.DORotate(new Vector3(0, 0, rotation), duration).SetEase(Ease.InCirc).OnComplete(() =>
                    {
                        Destroy(empty.gameObject);
                        Destroy(block.gameObject);
                    }));
                    i++;
                }

                seq.OnComplete(() =>
                {
                    var tween = AnimFX();
                    tween.Goto(tween.Duration(), true);
                    tween.PlayBackwards();
                });

                Tween AnimFX()
                {
                    var fxSeq = DOTween.Sequence();
                    fx.transform.localScale = Vector3.zero;
                    fxSeq.Insert(0, fx.DOFade(1, 0.3f));
                    fxSeq.Insert(0, fx.transform.DOScale(1f, 0.6f).SetEase(Ease.OutBack));
                    return fxSeq;
                }
            }
        }
    }
}