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
                for (var i = 0; i < indices.Count; i++)
                {
                    var toDestroy = new HashSet<Vector2Int>();
                    var toDestroyBlocks = new List<Block>();
                    
                    var index = indices[i];
                    var block = Grid.Get(index);
                    Grid.Set(index, block.next);
                    
                    var tr = block.transform;
                    if (tr.childCount > 0)
                    { 
                        tr.GetChild(0).SetParent(tr.parent, true);
                    }
                    
                    SetupToDestroyBlocks();
                    
                    tr.DOScale(1.3f, 0.4f).OnComplete(() =>
                    {
                        Instantiate(fx, tr.position, Quaternion.identity);
                        Destroy(tr.gameObject);
                        
                        SetupToDestroyBlocks();
                        var specialBlocks = fieldManager.GetSpecialBlocks(toDestroy);
                    
                        foreach (var (prefab, blocks) in specialBlocks)
                        {
                            var h = animator._handlers[prefab].handler as SpecialHandler;
                            h!.indices = blocks;
                            h.Handle();
                        }
                    
                        foreach (var ii in toDestroyBlocks)
                        {
                            Destroy(ii.gameObject);
                        }
                    });
                    
                
                    void SetupToDestroyBlocks()
                    {
                        DestroyBlock(index);
                        for (int j = 0; j < offsets.Count; j++)
                        {
                            DestroyBlock(index + offsets[j]);
                        }
                        
                        foreach (var ii in toDestroy)
                        {
                            var b = Grid.Get(ii);
                            if (b)
                            {
                                if (!FieldManager.SpecialBlockPrefabs.Contains(b.prefab))
                                { 
                                    toDestroyBlocks.Add(b);
                                    Grid.Set(ii, null);
                                }
                            }
                        }
                    }
                    
                    void DestroyBlock(Vector2Int ind)
                    {
                        if (!Grid.HasIndex(ind)) return;
                        var b = Grid.Get(ind);
                        if (b)
                        {
                            toDestroy.Add(ind);
                        }
                    }
                }
            }
        }
    }
}