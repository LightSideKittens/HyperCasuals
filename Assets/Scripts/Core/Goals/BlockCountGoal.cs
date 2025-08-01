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
            
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(World.IsPlaying) return;
            if (image.sprite != target.Block.sprite)
            { 
                image.sprite = target.Block.sprite;
                EditorUtility.SetDirty(image);
            }
        }
#endif
    }
}