using System.Collections.Generic;
using System.Text;

namespace FunGames.Analytics
{
    public static class FGAnalytics
    {
        public const int NO_SCORE = -1;
        public static FGAnalyticsCallbacks Callbacks => FGAnalyticsManager.Instance.Callbacks;

        public static void NewProgressionEvent(LevelStatus status, string prog01, int score = NO_SCORE)
        {
            FGAnalyticsManager.Instance.SendProgressionEvent(status, prog01, score);
        }

        public static void NewProgressionEvent(LevelStatus status, string prog01, string prog02, int score = NO_SCORE)
        {
            FGAnalyticsManager.Instance.SendProgressionEvent(status, prog01, prog02, score);
        }

        public static void NewProgressionEvent(LevelStatus status, string prog01, string prog02, string prog03,
            int score = NO_SCORE)
        {
            FGAnalyticsManager.Instance.SendProgressionEvent(status, prog01, prog02, prog03, score);
        }

        public static void NewDesignEvent(string eventId, float eventValue = 0)
        {
            FGAnalyticsManager.Instance.SendDesignEventSimple(eventId, eventValue);
        }

        public static void NewDesignEvent(string eventId, Dictionary<string, object> customFields, float eventValue = 0)
        {
            FGAnalyticsManager.Instance.SendDesignEventDictio(eventId, customFields, eventValue);
        }

        public static void NewAdEvent(AdAction adAction, AdType adType, string adSdkName, string adPlacement)
        {
            FGAnalyticsManager.Instance.SendAdEvent(adAction, adType, adSdkName, adPlacement);
        }

        public static string CreateEventId(params string[] strings)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strings.Length; i++)
            {
                sb.Append(strings[i]);
                if (strings.Length - 1 != i) sb.Append(":");
            }

            return sb.ToString();
        }
    }

    public enum LevelStatus
    {
        Start = 1,
        Complete = 2,
        Fail = 3
    };

    public enum AdAction
    {
        Clicked = 1,
        FailedShow = 2,
        Loaded = 3,
        Request = 4,
        RewardReceived = 5,
        Show = 6,
        Undefined = 7,
        Dismissed = 8,
        Impression = 9
    };

    public enum AdType
    {
        Banner = 1,
        Interstitial = 2,
        OfferWall = 3,
        Playable = 4,
        RewardedVideo = 5,
        Video = 6,
        Undefined = 7,
        AppOpen = 8,
        MREC = 9
    };
}