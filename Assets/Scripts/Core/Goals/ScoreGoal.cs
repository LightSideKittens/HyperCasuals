using LSCore;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations.Text;

namespace Core
{
    public class ScoreGoal : Goal
    {
        public LSNumber target;
        public LocalizationText comboText;
        public AnimSequencer reachedAnim;
        public AnimSequencer scoreAnim;
        public AnimSequencer comboAnim;
        private TextNumberAnim anim;
        private int lastScore;
        private bool needAnimate;
        
        protected virtual void Awake()
        {
            ScoreManager.ScoreChanged += OnScoreChanged;
            ScoreManager.ComboChanged += OnComboChanged;
            anim = scoreAnim.GetAnim<TextNumberAnim>();
        }

        private void Start()
        {
            ChangeScoreText((int)target.Number - ScoreManager.CurrentScore);
            lastScore = ScoreManager.LastScore;
        }

        private void OnDestroy()
        {
            ScoreManager.ScoreChanged -= OnScoreChanged;
            ScoreManager.ComboChanged -= OnComboChanged;
        }

        private void Update()
        {
            if (needAnimate)
            {
                anim.startValue = 0;
                anim.endValue = ScoreManager.CurrentScore - lastScore;
                scoreAnim.Animate();
                needAnimate = false;
            }
        }

        private void OnScoreChanged()
        {
            if (!needAnimate)
            {
                lastScore = ScoreManager.LastScore;
            }
            ChangeScoreText();
            needAnimate = true;
        }

        protected virtual void ChangeScoreText()
        {
            ChangeScoreText((int)target.Number - (ScoreManager.CurrentScore - ScoreManager.LastScore));
        }

        protected virtual void ChangeScoreText(int score)
        {
            target.Number = score;
            if (target.Number <= 0)
            {
                if (!IsReached)
                {
                    reachedAnim.Animate();
                }
                IsReached = true;
                target.Number = 0;
            }
        }

        private void OnComboChanged()
        {
            comboText.LocalizeArguments(ScoreManager.CurrentCombo);
            comboAnim.Animate();
        }
    }
}