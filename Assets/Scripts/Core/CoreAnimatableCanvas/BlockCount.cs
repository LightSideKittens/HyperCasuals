using System;
using Core;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using UnityEngine;

namespace Animatable
{
    [Serializable]
    public class BlockCount
    {
        public AnimComp blockCount;
        private OnOffPool<AnimComp> pool;
        
        internal void Init()
        {
            pool = new OnOffPool<AnimComp>(blockCount, CoreAnimatableCanvas.SpawnPoint, shouldStoreActive: true);
        }

        internal void ReleaseAll()
        {
            pool.ReleaseAll();
        }

        public static BlockCount Create(int count, Sprite sprite, Transform target)
        {
            var template = CoreAnimatableCanvas.BlockCount;
            
            var instance = template.pool.Get();
            var text = instance.GetComponentInChildren<LSNumber>();
            var image = instance.GetComponentsInChildren<LSImage>()[1];
            text.Number = count;
            image.sprite = sprite;
            instance.transform.localPosition = CoreAnimatableCanvas.GetLocalPosition(target.position);
            instance.Animate().OnComplete(() => template.pool.Release(instance));
            
            return new BlockCount()
            {
                blockCount = instance,
            };
        }
    }
}