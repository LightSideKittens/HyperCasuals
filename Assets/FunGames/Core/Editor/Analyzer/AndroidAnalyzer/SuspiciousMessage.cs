using System;
using UnityEditor.Build.Reporting;

namespace FunGames.Core.Editor.Analyzer
{
    [Serializable]
    public class SuspiciousMessage
    {
        public BuildReport Report;
        public BuildStep Step;
        public BuildStepMessage StepMessage;

        public SuspiciousMessage(BuildReport report, BuildStep step, BuildStepMessage message)
        {
            Report = report;
            Step = step;
            StepMessage = message;
        }
    }
}