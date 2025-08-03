namespace Core
{
    public class ClassicScoreGoal : ScoreGoal
    {
        protected override void ChangeScoreText()
        {
            target.Number = ScoreManager.CurrentScore;
        }
    }
}