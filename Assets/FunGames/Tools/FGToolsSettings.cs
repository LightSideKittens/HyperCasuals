using FunGames.Core.Utils;
using UnityEngine;

namespace FunGames.Tools
{
    [CreateAssetMenu(fileName = FGPath.ASSETS_RESOURCES + PATH, menuName = PATH, order = ORDER)]
    public class FGToolsSettings: FGModuleSettingsAbstract<FGToolsSettings>
    {
        public const string NAME = "FGToolsSettings";
        private const string PATH = FGPath.FUNGAMES + "/" + NAME;

        [Header("Xcode Post-build Automation")]
        [Tooltip("Add In-App Purchases entitlements to the Xcode project after iOS build completes")]
        public bool AddIAPCapability;
        
        [Tooltip("Add “App Uses Non-Exempt Encryption” value of \"NO\" to the Xcode project after iOS build completes")]
        public bool SetUsesNonExemptEncryptionFalse;

        [Tooltip("In Xcode, if “Privacy Tracking Domains” isn’t empty add “Privacy Tracking Enabled” and set it to YES")]
        public bool SetPrivacyInfo;
        
        [Tooltip("Set \"MARKETING_VERSION\" in Build Settings of the Unity target in Xcode to the value of " +
                 "Bundle version in Player Settings")]
        public bool SetMarketingVersion;

        [Tooltip(
            "Specify each flag that should be added to 'Other Linker Flags' in the Xcode project after iOS build completes")]
        public string[] AddOtherLinkerFlags;
        
        protected override FGToolsSettings LoadResources()
        {
            return Resources.Load<FGToolsSettings>(PATH);
        }
    }
}