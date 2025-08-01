using System;
using LSCore;
using LSCore.AnimationsModule;
using UnityEditor;

namespace Core
{
    public class BlockCountGoal : Goal
    {
        public LSNumber count;
        public AnimSequencer reachedAnim;
        public FieldAppearance.BlockData target;
        public LSImage image;
        public void Check(Block block)
        {
            if (block.prefab == target.Block)
            {
                if (count > 0)
                {
                    count -= 1;
                }
                else
                {
                    IsReached = true;
                    reachedAnim.Animate();
                }
            }
        }

        private void Awake()
        {
            UpdateSprite();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(World.IsPlaying) return;
            UpdateSprite();
        }
#endif

        private void UpdateSprite()
        {
            if (image.sprite != target.Block.sprite)
            { 
                image.sprite = target.Block.sprite;
            }
        }
    }
}