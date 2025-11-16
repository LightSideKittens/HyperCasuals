#if UNITY_EDITOR && UNITY_IOS

using UnityEditor.iOS.Xcode;

namespace FunGames.Tools.Editor
{
    public class XcodeCapabilitiesEditor
    {
        private readonly ProjectCapabilityManager _manager;
        
        public XcodeCapabilitiesEditor(string buildPath)
        {
            string pbxProjectPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);
            
            _manager = new ProjectCapabilityManager(
                pbxProjectPath,
                "Entitlements.entitlements",
                targetGuid: project.GetUnityMainTargetGuid()
            );
        }

        public void AddInAppPurchase()
        {
            _manager.AddInAppPurchase();
        }

        public void WriteToFile()
        {
            _manager.WriteToFile();
        }
    }
}

#endif