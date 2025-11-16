using System;

namespace FunGames.Core.Editor.Analyzer
{
    public class FGIssue
    {
        public string affectedSdk;
        public string title;
        public string issueDescription;
        public string howToFix;
        public FGSDKIssuePlatform platform;
        public FGSDKIssueSeverity severity;
        public FGSDKIssueTimeWindow timeWindow;
        public Action fix;
        public Action customAction;
        public Action customDescriptionDrawer;
        public string customActionText;
    }

    public enum FGSDKIssuePlatform
    {
        General,
        Android,
        IOS
    }

    public enum FGSDKIssueSeverity
    {
        Info,
        Warning,
        Error
    }

    public enum FGSDKIssueTimeWindow
    {
        BuildTime,
        Runtime,
    }
}