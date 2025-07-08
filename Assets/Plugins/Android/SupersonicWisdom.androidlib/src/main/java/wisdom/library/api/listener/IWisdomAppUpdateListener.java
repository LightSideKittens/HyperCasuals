package wisdom.library.api.listener;

public interface IWisdomAppUpdateListener {
    void onUpdateCheckResult(String versionCode);
    void onUpdateStarted(boolean didStart, String error);
}
