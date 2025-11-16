using System.Drawing;
using UnityEngine;

namespace FunGames.Tools.Utils
{
    public class AndroidSharedPreferences
    {
        private static AndroidJavaObject _sharedPreferences;

        public static AndroidJavaObject SharedPreferences
        {
            get
            {
                if (_sharedPreferences == null)
                {
                    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaClass preferenceManagerClass =
                        new AndroidJavaClass("android.preference.PreferenceManager");
                    _sharedPreferences =
                        preferenceManagerClass.CallStatic<AndroidJavaObject>("getDefaultSharedPreferences",
                            currentActivity);
                }

                return _sharedPreferences;
            }
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return SharedPreferences.Call<string>("getString", key, defaultValue);
        }
    }
}