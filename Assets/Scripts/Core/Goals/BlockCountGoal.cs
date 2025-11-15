using Animatable;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations;
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
                
                var imageAnim = ImageAnim.Create(block.sprite);
                var animImage = imageAnim.image;
                var animTransform = animImage.transform;
                var animComp = animTransform.GetComponent<AnimComp>();
                var localPosAnim = animComp.anim.GetAnim<LocalCurveAnim>();
                localPosAnim.startValue =  AnimatableCanvas.GetLocalPosition(block.transform.position);
                localPosAnim.endValue =  AnimatableCanvas.GetLocalPosition(image.transform.position);
                animComp.Animate().OnComplete(() =>
                {
                    image.transform.DOScale(1.4f, 0.5f).SetLoops(2, LoopType.Yoyo).KillOnDestroy();
                    imageAnim.Release(animImage);
                });
                
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
#if UNITY_EDITOR
            if(FieldAppearance.IsNull) return;
#endif
            if (image.sprite != target.Block.sprite)
            { 
                image.sprite = target.Block.sprite;
            }
        }
    }
}