using LSCore;
using LSCore.AnimationsModule;
using LSCore.Extensions;

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
                count -= 1;
                
                if (Check())
                {
                    reachedAnim.Animate();
                }
                
                FieldSave.SaveBlockGoal(target.id, count);
            }
        }

        private bool Check()
        {
            if (count <= 0)
            {
                count.Number = 0;
                IsReached = true;
            }
            
            return IsReached;
        }

        private void Awake()
        {
            UpdateSprite();
        }

        private void Start()
        {
            if (FieldSave.Exists)
            {
                var jBlockGoals = FieldSave.BlockGoals;
                if (jBlockGoals.TryGetValue(target.id, out var jCount))
                { 
                    count.Number = jCount.ToInt();
                }
                Check();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(World.IsPlaying) return;
            if(World.IsBuilding) return;
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