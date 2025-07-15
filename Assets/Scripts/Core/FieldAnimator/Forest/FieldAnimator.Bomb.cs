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
                    tr.GetChild(0).SetParent(tr.parent, true);
                    var toDestroy = new List<(Vector2Int index, Block block)>();
                    Grid.Set(d.index, d.block.next);

                    DestroyBlock(d.index);
                    for (int j = 0; j < offsets.Count; j++)
                    {
                        DestroyBlock(d.index + offsets[j]);
                    }

                    var specialBlocks = fieldManager.GetSpecialBlocks(toDestroy);
                    
                    foreach (var list in specialBlocks.Values)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            toDestroy.Remove(list[j]);
                        }
                    }

                    foreach (var (index, _) in toDestroy)
                    {
                        Grid.Set(index, null);
                    }
                    
                    tr.DOScale(1.3f, 0.4f).OnComplete(() =>
                    {
                        Instantiate(fx, tr.position, Quaternion.identity);
                        Destroy(tr.gameObject);
                        for (int j = 0; j < toDestroy.Count; j++)
                        {
                            Destroy(toDestroy[j].block.gameObject);
                        }
                        
                        foreach (var (prefab, list) in specialBlocks)
                        {
                            var h = animator.handlers[prefab].handler as SpecialHandler;
                            h!.blocks = list;
                            h.Handle();
                        }
                    });
                    
                    void DestroyBlock(Vector2Int index)
                    {
                        if (!Grid.HasIndex(index)) return;
                        var block = Grid.Get(index);
                        if (block)
                        {
                            var data = (index, block);
                            toDestroy.Add(data);
                            blocks.Remove(data);
                            fieldManager.RemoveData(data);
                        }
                    }
                }
            }
        }
    }
}