using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations.Text;
using UnityEngine;

namespace Core
{
    public class ScoreGoal : Goal
    {
        public LSNumber target;
        public LSNumber current;
        public LocalizationText comboText;
        public AnimSequencer scoreAnim;
        public AnimSequencer comboAnim;
        private TextNumberAnim anim;
        private int lastScore;
        private bool needAnimate;
        
        private void Awake()
        {
            ScoreManager.ScoreChanged += OnScoreChanged;
            ScoreManager.ComboChanged += OnComboChanged;
            anim = scoreAnim.GetAnim<TextNumberAnim>();
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
            current.text = $"{ScoreManager.CurrentScore}/";
            needAnimate = true;
        }

        private void OnComboChanged()
        {
            comboText.LocalizeArguments(ScoreManager.CurrentCombo);
            comboAnim.Animate();
        }

        public override bool IsReached => current.Number >= target.Number;
    }
}