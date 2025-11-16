using System.Collections.Generic;
using FunGames.Core.DatabaseModels;
using FunGames.Core.Editor.IAPIntegration;
using UnityEngine;

namespace FunGames.Core.Editor.IntegrationManager
{
    public static class IntegrationPostInstallProcessor
    {
        private const string UnityIAPModuleId = "fg_unity_iap";
        private const string GooglePlayLicensingParamId = "google_play_licensing_key";
        
        public static void OnInstallComplete(Dictionary<string, FGDBModuleSettings> modules)
        {
            modules.TryGetValue(UnityIAPModuleId, out FGDBModuleSettings fgUnityIapModule);
            if (fgUnityIapModule is {is_active: true}) OnInstallUnityIAP(fgUnityIapModule);
        }

        private static void OnInstallUnityIAP(FGDBModuleSettings moduleConfig)
        {
            ObfuscationMigration.MigrateObfuscations();
            
            if (ObfuscationGenerator.DoesAppleTangleClassExist() &&
                ObfuscationGenerator.DoesGooglePlayTangleClassExist()) return;

            string appleError = "";
            string googleError = "";

            foreach (var param in moduleConfig.parameters)
            {
                if (param.id != GooglePlayLicensingParamId) continue;

                string googlePlayLicensingKey = param.value;

                ObfuscationGenerator.ObfuscateSecrets(
                    true, ref appleError, ref googleError, googlePlayLicensingKey);

                if (!string.IsNullOrEmpty(appleError))
                    Debug.Log($"Error obfuscating Apple secret: {appleError}");
                if (!string.IsNullOrEmpty(googleError))
                    Debug.Log($"Error obfuscating Google Play secret: {googleError}");
                return;
            }
        }
    }
}