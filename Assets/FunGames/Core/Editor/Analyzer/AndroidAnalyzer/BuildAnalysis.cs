using System;
using System.Collections.Generic;

namespace FunGames.Core.Editor.Analyzer
{
    [Serializable]
    public class BuildAnalysis
    {
        public bool GradleBuildFileFound;
        public bool BuildReportFound;
        public bool BuildFailed;
        public List<ErrorGroup> DuplicateClassResults;
        public SuspiciousMessage DexLimitResult;

        public bool IsValid { get; private set; }
        public bool HasResults => DuplicateClassResults is {Count: > 0} || DexLimitResult?.Report != null;

        public BuildAnalysis(bool gradleBuildFileFound, bool buildReportFound, bool buildFailed,
            List<ErrorGroup> duplicateClassResults,
            SuspiciousMessage dexLimitResult
        )
        {
            GradleBuildFileFound = gradleBuildFileFound;
            BuildReportFound = buildReportFound;
            BuildFailed = buildFailed;
            DuplicateClassResults = duplicateClassResults;
            if (DuplicateClassResults != null && DuplicateClassResults.Count == 0)
                DuplicateClassResults = null;
            DexLimitResult = dexLimitResult;
            IsValid = true;
        }
    }
}