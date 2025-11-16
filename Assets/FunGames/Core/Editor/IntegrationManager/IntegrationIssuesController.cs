using System.Collections.Generic;
using System.Linq;
using FunGames.Core.Editor.Analyzer;
using FunGames.Tools.Utils;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class IntegrationIssuesController
    {
        public List<FGAnalyzer> TopLevelAnalyzers { get; private set; } = new();

        public void Initialize()
        {
            RefreshIssues();
        }
        
        public void RefreshIssues()
        {
            TopLevelAnalyzers = new List<FGAnalyzer>();
            Dictionary<string, FGAnalyzer> checkersMap = GetFGCheckersMap();

            foreach (var checker in checkersMap.Values)
            {
                if (checker.ParentId == "" || !checkersMap.ContainsKey(checker.ParentId))
                {
                    TopLevelAnalyzers.Add(checker);
                    continue;
                }

                FGAnalyzer parent = checkersMap[checker.ParentId];
                parent.Add(checker);
            }

            foreach (var checker in TopLevelAnalyzers)
            {
                checker.RunAnalysis();
            }
        }

        private Dictionary<string, FGAnalyzer> GetFGCheckersMap()
        {
            List<FGAnalyzer> checkers = ProjectUtils.GetEnumerableOfType<FGAnalyzer>();
            return checkers.ToDictionary(checker => checker.Id);
        }
    }
}