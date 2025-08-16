using System;
using System.Collections.Generic;
using LSCore.Extensions;
using UnityEngine;

namespace Core
{
    public partial class FieldAnimator
    {
        [Serializable]
        public class Stone : SpecialHandler
        {
            public Feel.SoundAndHaptic feel;
            public List<Sprite> stages;
            public ParticleSystem fx;
            private Dictionary<Vector2Int, Sprite> cache = new();
            private Dictionary<Vector2Int, Sprite> simulateCache;
            public override int Priority => 1;

            public override void Init()
            {
                base.Init();
                FieldManager.Saving += OnSaving;
                FieldManager.Loading += OnLoading;
            }

            public override void DeInit()
            {
                base.DeInit();
                FieldManager.Saving -= OnSaving;
                FieldManager.Loading -= OnLoading;
            }

            private void OnLoading()
            {
                var jGradeBlocks = FieldSave.GradeBlocks;

                for (int i = 0; i < jGradeBlocks.Count; i++)
                {
                    var gradeBlock = jGradeBlocks[i];
                    var index = gradeBlock["index"].ToVector2Int();
                    var stage = gradeBlock["stage"].ToInt();
                    cache[index] = stages[stage];
                }
            }

            private void OnSaving()
            {
                var dict = new Dictionary<Vector2Int, int>();
                foreach (var (index, sprite) in cache)
                {
                    dict.Add(index, stages.IndexOf(sprite));
                }
                FieldSave.SaveGradeBlocks(dict);
            }

            public override void StartSimulate()
            {
                base.StartSimulate();
                simulateCache = new Dictionary<Vector2Int, Sprite>(cache);
            }

            public override void StopSimulate()
            {
                base.StopSimulate();
                cache = simulateCache;
            }

            public override void Handle()
            {
                for (var i = 0; i < blocks.Count; i++)
                {
                    var index = blocks[i].index;
                    var block = Grid.Get(index);
                    if(!block) continue;
                    if (!cache.TryGetValue(index, out var sprite))
                    { 
                        sprite = block.sprite;
                        cache[index] = sprite;
                    }
                    
                    var stageIndex = stages.IndexOf(sprite);
                    if (stageIndex == -1)
                    {
                        continue;
                    }
                    stageIndex++;
                    Action action;
                    if (stageIndex == stages.Count)
                    {
                        cache.Remove(index);
                        action = () =>
                        {
                            if (block.next)
                            { 
                                block.next.transform.SetParent(block.transform.parent, true);
                            }
                            Destroy(block.gameObject);
                        };
                        Grid.Set(index, block.next);
                    }
                    else
                    {
                        sprite = stages[stageIndex];
                        action = () =>
                        {
                            if (block) block.sprite = sprite;
                        };
                        cache[index] = sprite;
                    }

                    action += () =>
                    {
                        if (block)
                        {
                            Instantiate(fx, block.transform.position, Quaternion.identity);
                        }
                    };
                    anim.Add((block, action));
                }
            }

            protected override void OnAnimate()
            {
                base.OnAnimate();
                feel.Do();
            }
        }
    }
}