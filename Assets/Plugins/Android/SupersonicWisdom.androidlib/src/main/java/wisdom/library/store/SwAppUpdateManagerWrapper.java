package wisdom.library.store;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.IntentSender;
import android.content.pm.PackageInfo;
import android.os.Build;

import com.google.android.gms.tasks.Task;
import com.google.android.play.core.appupdate.AppUpdateInfo;
import com.google.android.play.core.appupdate.AppUpdateManager;
import com.google.android.play.core.appupdate.AppUpdateManagerFactory;
import com.google.android.play.core.appupdate.AppUpdateOptions;
import com.google.android.play.core.install.InstallStateUpdatedListener;
import com.google.android.play.core.install.model.AppUpdateType;
import com.google.android.play.core.install.model.InstallStatus;
import com.google.android.play.core.install.model.UpdateAvailability;

import java.util.ArrayList;
import java.util.List;

import wisdom.library.api.listener.IWisdomAppUpdateListener;
import wisdom.library.util.SdkLogger;

public class SwAppUpdateManagerWrapper {
    private final AppUpdateManager appUpdateManager;
    private final List<IWisdomAppUpdateListener> appUpdateListeners = new ArrayList<>();
    private static final int REQUEST_CODE_UPDATE = 500;
    private static final String TAG = "SwAppUpdateManager";
    private static long currentVersionCode;
    private final Activity activity;

    public SwAppUpdateManagerWrapper(Activity activity) {
        this.activity = activity;
        appUpdateManager = AppUpdateManagerFactory.create(activity);
        currentVersionCode = getCurrentVersionCode(activity);

        InstallStateUpdatedListener installStateUpdatedListener = state -> {
            if (state.installStatus() == InstallStatus.DOWNLOADED) {
                popupDialogForCompleteUpdate();
            } else if (state.installStatus() == InstallStatus.DOWNLOADING) {
                long bytesDownloaded = state.bytesDownloaded();
                long totalBytesToDownload = state.totalBytesToDownload();
                SdkLogger.log(TAG, "Downloading update: " + bytesDownloaded + "/" + totalBytesToDownload);
            }
        };
        appUpdateManager.registerListener(installStateUpdatedListener);
    }

    public void addAppUpdateListener(IWisdomAppUpdateListener listener) {
        if (!appUpdateListeners.contains(listener)) {
            appUpdateListeners.add(listener);
            SdkLogger.log(TAG, "App update listener added.");
        }
    }

    public void removeAppUpdateListener(IWisdomAppUpdateListener listener) {
        appUpdateListeners.remove(listener);
        SdkLogger.log(TAG, "App update listener removed.");
    }

    public void checkForUpdate() {
        SdkLogger.log(TAG, "checkForUpdate called for version code: " + currentVersionCode);

        Task<AppUpdateInfo> appUpdateInfoTask = appUpdateManager.getAppUpdateInfo();
        appUpdateInfoTask
            .addOnSuccessListener(appUpdateInfo -> {
                int availability = appUpdateInfo.updateAvailability();

                if (availability == UpdateAvailability.DEVELOPER_TRIGGERED_UPDATE_IN_PROGRESS) {
                    // If an in-app update is already running, resume the update.
                    SdkLogger.log(TAG, "Update already running, resuming.");
                    try {
                        appUpdateManager.startUpdateFlowForResult(
                            appUpdateInfo,
                            activity,
                            AppUpdateOptions.defaultOptions(AppUpdateType.IMMEDIATE),
                            REQUEST_CODE_UPDATE
                        );
                    } catch (IntentSender.SendIntentException ignored) {}

                    return;
                }

                boolean isUpdateAvailable = availability == UpdateAvailability.UPDATE_AVAILABLE;
                SdkLogger.log(TAG, "Checking for app update, result: " + isUpdateAvailable);

                if (isUpdateAvailable) {
                    notifyUpdateCheckCompleted(String.valueOf(currentVersionCode));
                } else {
                    notifyUpdateCheckCompleted("");
                }
            })
            .addOnFailureListener(e -> {
                SdkLogger.error(TAG, "Failed to check for app update: " + e.getMessage(), e);
                notifyUpdateCheckCompleted("");
            })
            .addOnCompleteListener(task -> {
                if (!task.isSuccessful()) {
                    SdkLogger.error(TAG, "App update check task was not successful");
                    notifyUpdateCheckCompleted("");
                }
                SdkLogger.log(TAG, "App update check task completed");
            });
    }

    public void initiateUpdate(Activity activity, int updateType) {
        SdkLogger.log(TAG, "initiateUpdate called with update type " + updateType);

        Task<AppUpdateInfo> appUpdateInfoTask = appUpdateManager.getAppUpdateInfo();
        appUpdateInfoTask.addOnSuccessListener(appUpdateInfo -> {
            if (appUpdateInfo.updateAvailability() == UpdateAvailability.UPDATE_AVAILABLE
                && appUpdateInfo.isUpdateTypeAllowed(updateType)) {
                try {
                    boolean result = appUpdateManager.startUpdateFlowForResult(
                        appUpdateInfo,
                        activity,
                        AppUpdateOptions.defaultOptions(updateType),
                        REQUEST_CODE_UPDATE
                    );
                    notifyUpdateStarted(result, result ? null : "Failed to start update flow");
                } catch (IntentSender.SendIntentException e) {
                    notifyUpdateStarted(false, "Failed to initiate app update: " + e.getMessage());
                }
            } else {
                notifyUpdateStarted(false, "Update not available or type not allowed");
            }
        }).addOnFailureListener(e -> {
            notifyUpdateStarted(false, "Failed to get app update info: " + e.getMessage());
        });
    }

    private void notifyUpdateStarted(boolean didStart, String error) {
        SdkLogger.log(TAG, "App initialization started: didStart=" + didStart + ", error=" + error);

        for (IWisdomAppUpdateListener listener : appUpdateListeners) {
            listener.onUpdateStarted(didStart, error);
        }

        SdkLogger.log(TAG, "App initialization completed");
    }

    private void notifyUpdateCheckCompleted(String versionCode) {
        SdkLogger.log(TAG, "App update check called with version code: " + versionCode);

        for(IWisdomAppUpdateListener listener : appUpdateListeners) {
            listener.onUpdateCheckResult(versionCode);
        }

        SdkLogger.log(TAG, "App update check completed");
    }

    private long getCurrentVersionCode(Activity activity) {
        long currentVersionCode = 0;

        try {
            PackageInfo packageInfo = activity.getPackageManager().getPackageInfo(activity.getPackageName(), 0);

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                currentVersionCode = packageInfo.getLongVersionCode(); // getLongVersionCode() method for API 28 and above
            } else {
                currentVersionCode = packageInfo.versionCode;
            }
        } catch (Exception e) {
            SdkLogger.error(TAG, "Failed to get current version code: " + e.getMessage(), e);
        }

        return currentVersionCode;
    }

    private void popupDialogForCompleteUpdate() {
        activity.runOnUiThread(() -> {
            new AlertDialog.Builder(activity)
                .setTitle("Update Ready")
                .setMessage("An update has just been downloaded. Restart the app to complete the update or reschedule.")
                .setPositiveButton("Restart", (dialog, which) -> appUpdateManager.completeUpdate())
                .setNegativeButton("Not Now", (dialog, which) -> dialog.dismiss())
                .setCancelable(false)
                .show();
        });
    }
}
