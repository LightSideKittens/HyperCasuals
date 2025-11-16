using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using FunGames.Tools.Utils;
using Proyecto26;

public class AppstoreHandler : FGSingleton<AppstoreHandler>
{
#if UNITY_IPHONE
    	[DllImport ("__Internal")] private static extern void _OpenAppInStore(int appID);
#endif

#if UNITY_ANDROID
    private static AndroidJavaObject jo;
#endif

    void Awake()
    {
        if (!Application.isEditor)
        {
#if UNITY_ANDROID
            jo = new AndroidJavaObject("com.purplelilgirl.nativeappstore.NativeAppstore");
#endif
        }
        else
        {
            Debug.Log("AppstoreHandler:: Cannot open Appstore in Editor.");
        }
    }

    public void openAppInStore(string appID)
    {
        if (!Application.isEditor)
        {
#if UNITY_IPHONE
    			int appIDIOS;
    			if(int.TryParse(appID, out appIDIOS))
    			{
					_OpenAppInStore(appIDIOS);
    			}
#endif

#if UNITY_ANDROID
            jo.Call("OpenInAppStore", "market://details?id=" + appID);
#endif
        }
        else
        {
            Debug.Log("AppstoreHandler:: Cannot open Appstore in Editor.");
        }
    }

    public void openAppInStore()
    {
#if UNITY_IOS
        RestClient.Get("https://itunes.apple.com/lookup?bundleId=" + Application.identifier)
            .Then(response =>
            {
                JSONNode node = JSON.Parse(response.Text);
                JSONNode trackViewUrl = SimpleJsonHelpers.FindNode(node, "trackId");
                string appID = trackViewUrl != null ? trackViewUrl.Value : String.Empty;
                openAppInStore(appID);
            });
#elif UNITY_ANDROID
        openAppInStore(Application.identifier);
#endif
    }

    public void appstoreClosed()
    {
        Debug.Log("AppstoreHandler:: Appstore closed.");
    }
}