namespace FunGames.Core
{
    public class FunGamesSDK
    {
        public static FGCoreCallbacks Callbacks => FGCore.Instance.Callbacks;

        public static bool IsInitialized => FGCore.Instance.IsInitialized;
        public static void RemoveAds(params FGAdType[] ads) => FGCore.Instance.RemoveAds(ads);
        public static void RestoreAds() => FGCore.Instance.RestoreAds();
        public static bool IsNoAd(FGAdType ad) => FGCore.Instance.IsNoAd(ad);
        public static bool IsFirstConnection => FGCore.Instance.IsFirstConnection();
        public static bool IsConnectedToInternet => FGCore.Instance.HasInternetConnection();
        public static int DaysSinceFirstCo => FGCore.Instance.DaysSinceFirstConnection();
        public static int DaysSinceLastCo => FGCore.Instance.DaysSinceLastConnection();
        public static int CurrentSessionNumber => FGCore.Instance.GetCurrentSessionNumber();
    }
}