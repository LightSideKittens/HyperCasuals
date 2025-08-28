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
            set
            {
                if(!IsEnabled) return;
                Goals["time"] = value;
            }
        }

        public static void SaveScoreManager(
            int lastScore,
            int currentScore,
            int currentCombo,
            int currentTurn,
            int turnsForBonus)
        {
            if(!IsEnabled) return;
            
            var jScoreManager = Goals.AsJ<JObject>("scoreManager");
            jScoreManager["lastScore"] = lastScore;
            jScoreManager["currentScore"] = currentScore;
            jScoreManager["currentCombo"] = currentCombo;
            jScoreManager["currentTurn"] = currentTurn;
            jScoreManager["turnsForBonus"] = turnsForBonus;
        }
        
        public static void LoadScoreManager(
            ref int lastScore,
            ref int currentScore,
            ref int currentCombo,
            ref int currentTurn,
            ref int turnsForBonus)
        {
            if(!IsEnabled) return;
            
            var jScoreManager = Goals.AsJ<JObject>("scoreManager");
            if (jScoreManager.ContainsKey("lastScore"))
            {
                lastScore = jScoreManager["lastScore"].ToInt();
                currentScore = jScoreManager["currentScore"].ToInt();
                currentCombo = jScoreManager["currentCombo"].ToInt();
                currentTurn = jScoreManager["currentTurn"].ToInt();
                turnsForBonus = jScoreManager["turnsForBonus"].ToInt();
            }
        }

        public static JObject BlockGoals => Goals.AsJ<JObject>("blockGoals");
        public static void SaveBlockGoal(string blockPrefab, int count)
        {
            if(!IsEnabled) return;
            
            var jBlockGoals = BlockGoals;
            jBlockGoals[blockPrefab] = count;
        }
    }
}