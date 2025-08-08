using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace Core
{
    public partial class FieldAnimator
    {
        [Serializable]
        public abstract class BaseDestroyer : SpecialHandler
        {
            public Feel.SoundAndHaptic feel;
            public abstract List<Vector2Int> Offsets { get; } 
            public static HashSet<Block> handled = new();
            public static HashSet<Block> simulateHandled;
            
            public override void StartSimulate()
            {
                base.StartSimulate();
                simulateHandled = new HashSet<Block>(handled);
            }

            public override void StopSimulate()
            {
                base.StopSimulate();
                handled = simulateHandled;
            }

            public override void Handle()
            {
                var offsets = Offsets;
                for (var i = 0; i < blocks.Count; i++)
                {
                    var index = blocks[i].index;
                    var block = blocks[i].block;
                    if(!block) continue;
                    if (!handled.Add(block))
                    {
                        continue;
                    }
                    Grid.Set(index, block.next);
                    
                    var tr = block.transform;
                    
                    var toDestroy = new HashSet<Vector2Int>();
                    var toDestroyBlocks = new List<Block>();   
                    SetupToDestroyBlocks();
                    var specialBlocks = FieldManager.GetSpecialBlocks(toDestroy);
                    List<(Block block, Action action)> internalAnim = new();
                    
                    foreach (var (prefab, blockss) in specialBlocks)
                    {
                        var h = animator._handlers[prefab].handler as SpecialHandler;
                        var lastBlocks = h!.blocks;
                        var lastAnim = h.anim;
                        if (h is BaseDestroyer)
                        {
                            blockss.RemoveAll(x => animator.ContainsInSpecialBlocks(x.block));
                        }
                        h.blocks = blockss;
                        h.anim = new List<(Block block, Action action)>();
                        h.Handle();
                        foreach (var kvp in h.anim)
                        {
                            internalAnim.Add(kvp);
                        }

                        h.anim = lastAnim;
                        h.blocks = lastBlocks;
                    }

                    anim.Add((block, () =>
                    {
                        if (tr.childCount > 0)
                        {
                            var b = tr.FindComponentInChildren<Block>();
                            if (b)
                            {
                                b.transform.SetParent(tr.parent, true);
                            }
                        }
                        
                        AnimateDestroying(block, toDestroyBlocks, internalAnim);
                    }));
                    
                
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
                                if (!b.IsSpecial)
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

            protected abstract void AnimateDestroying(Block self, List<Block> toDestroy,
                List<(Block block, Action action)> specialBlockAnims);

            protected override void OnAnimate()
            {
                base.OnAnimate();
                feel.Do();
            }
        }

        [Serializable]
        public class Bomb : BaseDestroyer
        {
            public ParticleSystem fx;
            public List<Vector2Int> offsets;
            public override List<Vector2Int> Offsets => offsets;

            protected override void AnimateDestroying(Block self, List<Block> toDestroy, List<(Block block, Action action)> specialBlockAnims)
            {
                var tr = self.transform;
                var factor = tr.localScale.x;
                tr.DOScale(factor * 1.3f, 0.4f).OnComplete(() =>
                {
                    Instantiate(fx, tr.position, Quaternion.identity);
                    Destroy(tr.gameObject);
                    foreach (var data in specialBlockAnims)
                    {
                        data.action?.Invoke();
                    }
                    foreach (var ii in toDestroy)
                    {
                        Destroy(ii.gameObject);
                    }
                });
            }
        }

        [Serializable]
        public class Rocket : BaseDestroyer
        {
            public LSVector2.Axis axis;
            public override List<Vector2Int> Offsets { get; } = new();
            public GameObject leftRocket;
            public GameObject rightRocket;

            public override void Handle()
            {
                var size = FieldManager.Grid.GetSize(); 
                var offset = new Vector2Int(0, 0);
                Offsets.Clear();
                if (axis == LSVector2.Axis.X)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        offset.x = x+1;
                        Offsets.Add(offset);
                        offset.x = -x-1;
                        Offsets.Add(offset);
                    }
                }

                if (axis == LSVector2.Axis.Y)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        offset.y = y + 1;
                        Offsets.Add(offset);
                        offset.y = -y - 1;
                        Offsets.Add(offset);
                    }
                }
                
                base.Handle();
            }

            protected override void AnimateDestroying(Block self, List<Block> toDestroy, List<(Block block, Action action)> specialBlockAnims)
            {
                var tr = self.transform;
                Destroy(tr.GetChild(0).gameObject);
                var left = Instantiate(leftRocket, tr.position, tr.rotation).transform;
                var right = Instantiate(rightRocket, tr.position, tr.rotation).transform;
                var scale = tr.localScale.x.ToVector3();
                left.localScale = scale;
                right.localScale = scale;
                var copiedToDestroy = new List<Block>(toDestroy);
                var copiedSpecialBlockAnims = new List<(Block block, Action action)>(specialBlockAnims);
                self.render.enabled = false;
                left.DOMove(left.position + left.right * -25, 1f).SetUpdate(UpdateType.Fixed);
                right.DOMove(right.position + right.right * 25, 1f).SetUpdate(UpdateType.Fixed).OnUpdate(() =>
                {
                    for (var i = 0; i < copiedToDestroy.Count; i++)
                    {
                        var block = copiedToDestroy[i];
                        if (Check(block))
                        {
                            copiedToDestroy.RemoveAt(i--);
                            Destroy(block.gameObject);
                        }
                    }

                    for (var i = 0; i < copiedSpecialBlockAnims.Count; i++)
                    {
                        var data = copiedSpecialBlockAnims[i];
                        
                        if (Check(data.block))
                        {
                            copiedSpecialBlockAnims.RemoveAt(i--);
                            data.action?.Invoke();
                        }
                    }
                    bool Check(Block block)
                    {
                        if(!block) return false;
                        var toRight = (block.transform.position - right.position).magnitude;
                        var toLeft = (block.transform.position - left.position).magnitude;
                        return toRight < 1f || toLeft < 1f;
                    }
                }).OnComplete(() =>
                {
                    Destroy(self.gameObject);
                    Destroy(left.gameObject);
                    Destroy(right.gameObject);
                });
            }
        }
    }
}