using System;
using FunGames.Core.Utils;
using UnityEngine;

namespace FunGames.Core
{
    [CreateAssetMenu(fileName = FGPath.ASSETS_RESOURCES + PATH, menuName = PATH, order = ORDER)]
    public class FGMainSettings : FGModuleSettingsAbstract<FGMainSettings>
    {
        public const string NAME = "FGMainSettings";
        private const string PATH = FGPath.FUNGAMES + "/" + NAME;

        public string ApiKey = String.Empty;
        public LogLevel LogLevel = LogLevel.Verbose;
        public bool AmazonStoreSetup = false;

        protected override FGMainSettings LoadResources()
        {
            return Resources.Load<FGMainSettings>(PATH);
        }
    }
}