using System;
using System.Collections.Generic;
using System.Linq;

namespace FunGames.Core.Editor.Analyzer
{
    public static class DexLimitErrorAnalyzer
    {
        public static bool HasKnownError(string message)
        {
            return message.IndexOf(" dex file", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static SuspiciousMessage AnalyzeMessages(List<SuspiciousMessage> messages, LogCallback logCallback)
        {
            return messages.FirstOrDefault(
                msg => msg.StepMessage.content.Contains("single dex file") 
                       || msg.StepMessage.content.Contains("dex: method ID not"));
        }
    }
}