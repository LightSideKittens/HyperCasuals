using Firebase.Analytics;
using LSCore;

namespace Core
{
    public class ClassicScoreGoal : ScoreGoal
    {
        public LSNumber bestScore;

        protected override void Awake()
        {
            base.Awake();
            bestScore.Number = GameSave.BestScore;
        }

        protected override void ChangeScoreText()
        {
            var currentScore = ScoreManager.CurrentScore;
            target.Number = currentScore;
            if (currentScore > GameSave.BestScore)
            {
                GameSave.BestScore = currentScore;
                bestScore.Number = currentScore;
            }
        }
    }
}