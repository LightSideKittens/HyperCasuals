// using FunGames.Analytics;
// using FunGames.UserConsent;
// using FunGames.RemoteConfig;
using FunGames.Tools.Utils;
using UnityEngine;
using UnityEngine.UI;

public class FGUpdatePopup : MonoBehaviour
{
    public GameObject popUp;
    public Button UpdateAppButton;
    public Button CloseButton;

    private const string RC_UPDATE_POPUP_ACTIVATED = "FGVersionToBeUpdated";
        private const string RC_UPDATE_POPUP_CLOSABLE= "FGUpdatePopupClosable";

    // Start is called before the first frame update
    void Awake()
    {
        // FGRemoteConfig.AddDefaultValue(RC_UPDATE_POPUP_ACTIVATED, Application.version);
        // FGRemoteConfig.AddDefaultValue(RC_UPDATE_POPUP_CLOSABLE, 1);
        // FGUserConsent.OnComplete += ActivatePopup;
        popUp.SetActive(false);
        CloseButton.gameObject.SetActive(false);
        UpdateAppButton.onClick.AddListener(GoToStore);
        CloseButton.onClick.AddListener(Close);
    }

    private void ActivatePopup()
    {
        if (IsAppUpToDate()) return;
        if (popUp.activeSelf) return;

        popUp.SetActive(true);
        // CloseButton.gameObject.SetActive(FGRemoteConfig.GetBooleanValue(RC_UPDATE_POPUP_CLOSABLE));
        // FGAnalytics.NewDesignEvent("UpdatePopupDisplayed");
    }

    private void GoToStore()
    {
        AppstoreHandler.Instance.openAppInStore();
        // if(FGRemoteConfig.GetBooleanValue(RC_UPDATE_POPUP_CLOSABLE)) Close();
    }

    private void Close()
    {
        popUp.SetActive(false);
    }

    private bool IsAppUpToDate()
    {
        return true;
        // string currentVersion = Application.version;
        // string versionToUpdate = FGRemoteConfig.GetStringValue(RC_UPDATE_POPUP_ACTIVATED);
        //
        // CompareVersionResult result = VersionUtils.CompareVersions(currentVersion, versionToUpdate);
        // switch (result)
        // {
        //     case CompareVersionResult.SecondIsGreater:
        //         return false;
        //     default:
        //         return true;
        // }

        // if (currentVersion.Equals(versionToUpdate)) return true;
        //
        // string[] currentVersionSplit = currentVersion.Split('.');
        // string[] versionToUpdateSplit = versionToUpdate.Split('.');
        //
        // string[] smallestVersionString = currentVersionSplit.Length < versionToUpdateSplit.Length
        //     ? currentVersionSplit
        //     : versionToUpdateSplit;
        //
        // for (int i = 0; i < smallestVersionString.Length; i++)
        // {
        //     if (Convert.ToInt32(currentVersionSplit[i]) < Convert.ToInt32(versionToUpdateSplit[i])) return false;
        // }
        //
        // return true;
    }
}