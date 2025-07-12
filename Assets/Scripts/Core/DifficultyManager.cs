using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("Настройка сложности")]
    public AnimationCurve difficultyCurve;

    [Header("Границы уровней")]
    public int mediumThreshold = 10;
    public int hardThreshold = 20;

    private int placedShapesCount = 0;
    private DifficultyLevel currentLevel = DifficultyLevel.Easy;

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public void OnShapePlaced()
    {
        placedShapesCount++;
        UpdateDifficultyLevel();
    }

    public float GetDifficultyValue()
    {
        float normalized = Mathf.InverseLerp(0, hardThreshold, placedShapesCount);
        return difficultyCurve.Evaluate(normalized);
    }

    public DifficultyLevel GetCurrentDifficultyLevel()
    {
        return currentLevel;
    }

    private void UpdateDifficultyLevel()
    {
        DifficultyLevel previous = currentLevel;

        if (placedShapesCount >= hardThreshold)
        {
            currentLevel = DifficultyLevel.Hard;
        }
        else if (placedShapesCount >= mediumThreshold)
        {
            currentLevel = DifficultyLevel.Medium;
        }
        else
        {
            currentLevel = DifficultyLevel.Easy;
        }

        if (previous != currentLevel)
        {
            Debug.Log($"Переход на уровень сложности: {currentLevel}");
        }
    }

    public void ResetDifficulty()
    {
        placedShapesCount = 0;
        currentLevel = DifficultyLevel.Easy;
    }
}