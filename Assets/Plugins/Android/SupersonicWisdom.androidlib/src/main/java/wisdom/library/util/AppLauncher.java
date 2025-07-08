package wisdom.library.util;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Intent;

public class AppLauncher {

    private static final String MAIN_ACTIVITY_CLASS_NAME = "com.unity3d.player.UnityPlayerActivity";

    public boolean openApplication(Activity mActivity, String appIdentifier) {
        try {
            if (mActivity == null) {
                SdkLogger.error("WisdomSDK", "Current activity is null");
                return false;
            }
            
            SdkLogger.log("OpenApplication with package id " + appIdentifier);

            Intent launchIntent = createIntent(appIdentifier);
            mActivity.startActivity(launchIntent);
            
            SdkLogger.log("WisdomSDK", "Application started successfully.");
            
            return true;
            
        } catch (Exception e) {
            SdkLogger.error("WisdomSDK", "Failed to open application with package id " + appIdentifier, e);
            return false;
        }
    }

    private Intent createIntent(String appIdentifier) {
        Intent intent = new Intent();
        intent.setComponent(new ComponentName(appIdentifier, MAIN_ACTIVITY_CLASS_NAME));
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP);
        return intent;
    }
}
