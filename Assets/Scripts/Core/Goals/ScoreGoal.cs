using LSCore;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations.Text;

namespace Core
{
    public class ScoreGoal : Goal
    {
        public LSNumber target;
        public LSText current;
        public LSNumber multiplier;
        public AnimSequencer scoreAnim;
        private TextNumberAnim anim;
        private bool needAnimate;
        private int currentScore;
        
        private void Awake()
        {
            Block.Destroyed += OnBlockDestroyed;
            anim = scoreAnim.GetAnim<TextNumberAnim>();
        }

        private void OnDestroy()
        {
            Block.Destroyed -= OnBlockDestroyed;
        }
        
        private void OnBlockDestroyed(Block block)
        {
            if(block.IsSpecial) return;
            currentScore += multiplier;
            current.text = $"{currentScore}/";
            needAnimate = true;
        }

        private void Update()
        {
            if (needAnimate)
            {
                anim.endValue = currentScore;
                scoreAnim.Animate();
                needAnimate = false;
            }
        }
    }
}