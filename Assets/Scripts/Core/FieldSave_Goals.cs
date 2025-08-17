using LSCore.Extensions;
using Newtonsoft.Json.Linq;

namespace Core
{
    public static partial class FieldSave
    {
        public static JObject Goals => Config.AsJ<JObject>("goals");

        public static long Time
        {
            get => Goals["time"].ToLong();
            set => Goals["time"] = value;
        }

        public static void SaveScoreManager(
            int lastScore,
            int currentScore,
            int currentCombo,
            int currentTurn,
            int turnsForBonus)
        {
            var jScoreManager = Goals.AsJ<JObject>("scoreManager");
            jScoreManager["lastScore"] = lastScore;
            jScoreManager["currentScore"] = currentScore;
            jScoreManager["currentCombo"] = currentCombo;
            jScoreManager["currentTurn"] = currentTurn;
            jScoreManager["turnsForBonus"] = turnsForBonus;
        }
        
        public static void LoadScoreManager(
            out int lastScore,
            out int currentScore,
            out int currentCombo,
            out int currentTurn,
            out int turnsForBonus)
        {
            var jScoreManager = Goals.AsJ<JObject>("scoreManager");
            lastScore = jScoreManager["lastScore"].ToInt();
            currentScore = jScoreManager["currentScore"].ToInt();
            currentCombo = jScoreManager["currentCombo"].ToInt();
            currentTurn = jScoreManager["currentTurn"].ToInt();
            turnsForBonus = jScoreManager["turnsForBonus"].ToInt();
        }

        public static JObject BlockGoals => Goals.AsJ<JObject>("blockGoals");
        public static void SaveBlockGoal(string blockPrefab, int count)
        {
            var jBlockGoals = BlockGoals;
            jBlockGoals[blockPrefab] = count;
        }
    }
}