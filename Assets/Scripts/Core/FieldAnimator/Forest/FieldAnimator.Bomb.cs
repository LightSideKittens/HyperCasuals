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
        public class Bomb : SpecialHandler
        {
            public ParticleSystem fx;
            public List<Vector2Int> offsets;

            public override void Handle()
            {
                for (var i = 0; i < blocks.Count; i++)
                {
                    var d = blocks[i];
                    var tr = d.block.transform;
                    tr.DOScale(1.3f, 0.4f).OnComplete(() =>
                    {
                        Instantiate(fx, tr.position, Quaternion.identity);
                        Destroy(tr.gameObject);
                        DestroyBlock(d.index);
                        for (int j = 0; j < offsets.Count; j++)
                        {
                            DestroyBlock(d.index + offsets[j]);
                        }

                        void DestroyBlock(Vector2Int index)
                        {
                            if (!fieldManager.grid.HasIndex(index)) return;
                            var block = fieldManager.grid.Get(index);
                            if (block)
                            { 
                                Destroy(block.gameObject);
                            }
                            fieldManager.grid.Set(index, null);
                        }
                    });
                }
            }
        }
    }
}