using TMPro;
using UnityEngine;


public class ScoreManager : MonoBehaviour
{
    public TMP_Text textScore;
    private int score;
    public int pointsPerLine = 30;
    public bool enableComboBonus;
    public bool enableSpeedBonus;
    private int comboMultiplier = 1;
    private int comboAttempts = 0;
    private const int maxAttemptsWithoutBreak = 6;
    private float lastBreakTime = 999f;
    private const float speedBonusWindow = 5f;

    private void Start()
    {
        score = 0;
        UpdateText();
    }

    public void AddScore(int linesDestroyed, bool wasLineBroken)
    {
        if (enableComboBonus)
        {
            if (wasLineBroken)
            {
                comboAttempts = 0;
                comboMultiplier++;
            }
            else
            {
                comboAttempts++;
                if (comboAttempts >= maxAttemptsWithoutBreak)
                {
                    comboMultiplier = 1;
                    comboAttempts = 0;
                }
            }
        }
        else
        {
            comboMultiplier = 1;
        }

        int baseScore = linesDestroyed * pointsPerLine;
        int finalScore = baseScore * comboMultiplier;

        string bonusInfo = $"Combo x{comboMultiplier}";

        if (enableSpeedBonus && wasLineBroken)
        {
            float timeNow = Time.time;
            float timeSinceLastBreak = timeNow - lastBreakTime;

            if (lastBreakTime > 0 && timeSinceLastBreak <= speedBonusWindow)
            {
                finalScore += 50; // фиксированный бонус
                bonusInfo += $" + SpeedBonus (+50 in {timeSinceLastBreak:F2}s)";
            }
            else
            {
                bonusInfo += $" (no speed bonus, {timeSinceLastBreak:F2}s)";
            }

            lastBreakTime = timeNow;
        }

        if (wasLineBroken && !enableSpeedBonus)
            lastBreakTime = Time.time;

        score += finalScore;

        // ДЕБАГ
        //Debug.Log($"[SCORE] Lines: {linesDestroyed}, Base: {baseScore}, {bonusInfo} → +{finalScore} → Total: {score}");

        UpdateText();
    }


    private void UpdateText()
    {
        if (textScore != null)
        {
            textScore.text = score.ToString();
        }
    }
}