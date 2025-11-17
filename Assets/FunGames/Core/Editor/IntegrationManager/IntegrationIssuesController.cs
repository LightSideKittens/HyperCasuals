using System.Collections.Generic;
using System.Linq;
using FunGames.Core.Editor.Analyzer;
using FunGames.Tools.Utils;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class IntegrationIssuesController
    {
        public static List<FGAnalyzer> TopLevelAnalyzers { get; private set; } = new();

        public void Initialize()
        {
            RefreshIssues();
        }
        
        public void RefreshIssues()
        {
            DisposeAnalyzers();
            TopLevelAnalyzers = new List<FGAnalyzer>();
            Dictionary<string, FGAnalyzer> analyzersMap = GetFGAnalyzersMap();

            foreach (var analyzer in analyzersMap.Values)
            {
                if (analyzer.ParentId == "" || !analyzersMap.ContainsKey(analyzer.ParentId))
                {
                    TopLevelAnalyzers.Add(analyzer);
                    continue;
                }

                FGAnalyzer parent = analyzersMap[analyzer.ParentId];
                parent.Add(analyzer);
            }

            foreach (var analyzer in TopLevelAnalyzers)
            {
                analyzer.RunAnalysis();
            }
        }

        private Dictionary<string, FGAnalyzer> GetFGAnalyzersMap()
        {
            List<FGAnalyzer> checkers = ProjectUtils.GetEnumerableOfType<FGAnalyzer>();
            return checkers.ToDictionary(checker => checker.Id);
        }

        private void DisposeAnalyzers()
        {
            foreach (var analyzer in TopLevelAnalyzers)
            {
                analyzer.Dispose();
            }
        }
    }
}