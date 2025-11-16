using System;
using System.Collections.Generic;
using System.Linq;

namespace FunGames.Core.Editor.IntegrationManager
{
    [Serializable]
    public class FGSDKInstallationManifest
    {
        public List<string> enabledPluginNames = new();
        public List<string> externalPackagesToImport = new();

        public void Clear()
        {
            enabledPluginNames.Clear();
            externalPackagesToImport.Clear();
        }

        public void CopyFrom(FGSDKInstallationManifest source)
        {
            enabledPluginNames = source.enabledPluginNames.ToList();
            externalPackagesToImport = source.externalPackagesToImport.ToList();
        }
    }
}