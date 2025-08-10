using LSCore;

namespace Core
{
    public class ClassicScoreGoal : ScoreGoal
    {
        public LSNumber bestScore;

        protected override void Awake()
        {
            base.Awake();
            bestScore.Number = CoreWorld.BestScore;
        }

        protected override void ChangeScoreText()
        {
            var currentScore = ScoreManager.CurrentScore;
            target.Number = currentScore;
            if (currentScore > CoreWorld.BestScore)
            {
                CoreWorld.BestScore = currentScore;
                bestScore.Number = currentScore;
            }
        }
    }
}