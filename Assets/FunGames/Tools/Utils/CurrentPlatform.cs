using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
using Unity.Advertisement.IosSupport;
#endif

namespace FunGames.Tools.Utils
{
    public class CurrentPlatform
    {
        public static bool Is(Platform platform)
        {
#if UNITY_EDITOR
            return Platform.Editor.Equals(platform);
#elif UNITY_IOS
            return Platform.IOS.Equals(platform);
#elif UNITY_ANDROID
            return Platform.Android.Equals(platform);
#endif
            return false;
        }

        public static bool IsFireOS
        {
            get
            {
                string deviceModel = SystemInfo.deviceModel;
                bool isAmazon = deviceModel.Contains("Amazon") || deviceModel.Contains("Fire");
                return isAmazon;
            }
        }

        public static void GetAdvertisingId(out string advertisingID, out bool limitAdvertising)
        {
#if UNITY_EDITOR
            advertisingID = "";
            limitAdvertising = false;
#elif UNITY_ANDROID
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass client =
                new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            AndroidJavaObject adInfo =
                client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);

            advertisingID = adInfo.Call<string>("getId").ToString();
            limitAdvertising = (adInfo.Call<bool>("isLimitAdTrackingEnabled"));
#elif UNITY_IOS
                limitAdvertising = ATTrackingStatusBinding.GetAuthorizationTrackingStatus() !=
                                   ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
                advertisingID = Device.advertisingIdentifier;
#endif
        }
    }

    public enum Platform
    {
        Android,
        IOS,
        FireOS,
        Editor
    }
}