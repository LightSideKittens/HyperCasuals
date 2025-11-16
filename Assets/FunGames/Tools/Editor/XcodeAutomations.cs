#if UNITY_EDITOR && UNITY_IOS

using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace FunGames.Tools.Editor
{
    public class XcodeAutomations : IPostprocessBuildWithReport
    {
        public int callbackOrder { get; }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;

            FGToolsSettings settings = new FGToolsPackage().GetModuleSettings<FGToolsSettings>();
            PatchXcodeBuild(settings, report.summary.outputPath);
        }

        private void PatchXcodeBuild(FGToolsSettings settings, string buildOutputPath)
        {
            string pbxProjectPath = PBXProject.GetPBXProjectPath(buildOutputPath);

            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);

            PatchLinkerFlags(pbxProjectPath, project, settings);
            PatchMarketingVersion(pbxProjectPath, project, settings);
            PatchPrivacyInfo(buildOutputPath, settings);
            PatchInfoPlist(buildOutputPath, settings);
        }

        private void PatchLinkerFlags(string pbxProjectPath, PBXProject project, FGToolsSettings settings)
        {
            if (settings.AddOtherLinkerFlags.Length == 0) return;

            string mainTargetGuid = project.GetUnityMainTargetGuid();
            string frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

            foreach (var flag in settings.AddOtherLinkerFlags)
            {
                // Add the flag to Other Linker Flags for the main target
                project.AddBuildProperty(mainTargetGuid, "OTHER_LDFLAGS", flag);

                // Add the flag to Other Linker Flags for the UnityFramework target
                project.AddBuildProperty(frameworkTargetGuid, "OTHER_LDFLAGS", flag);

            }

            project.WriteToFile(pbxProjectPath);
        }

        private void PatchMarketingVersion(string pbxProjectPath, PBXProject project, FGToolsSettings settings)
        {
            if (!settings.SetMarketingVersion) return;

            string mainTargetGuid = project.GetUnityMainTargetGuid();
            project.SetBuildProperty(mainTargetGuid, "MARKETING_VERSION", PlayerSettings.bundleVersion);
            project.WriteToFile(pbxProjectPath);
        }

        private void PatchPrivacyInfo(string buildOutputPath, FGToolsSettings settings)
        {
            if(!settings.SetPrivacyInfo) return;
            
            string privacyInfoPath = Path.Combine(buildOutputPath, "UnityFramework/PrivacyInfo.xcprivacy");

            if (!File.Exists(privacyInfoPath)) return;
            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(privacyInfoPath);

            // Find the "Privacy Tracking Domains" element
            XmlNode trackingDomainsNode =
                xmlDoc.SelectSingleNode("//key[. = 'Privacy Tracking Domains']/following-sibling::array");

            if (trackingDomainsNode is not {HasChildNodes: true}) return;
            
            // Check if "Privacy Tracking Enabled" already exists
            XmlNode privacyTrackingEnabledNode =
                xmlDoc.SelectSingleNode("//key[. = 'Privacy Tracking Enabled']");
            if (privacyTrackingEnabledNode == null)
            {
                XmlElement keyElement = xmlDoc.CreateElement("key");
                keyElement.InnerText = "Privacy Tracking Enabled";
                XmlElement trueElement = xmlDoc.CreateElement("true");

                // Add the new elements at the end of the root element
                XmlNode dictNode = xmlDoc.SelectSingleNode("//dict");
                dictNode.AppendChild(keyElement);
                dictNode.AppendChild(trueElement);
            }

            // Save the modified PrivacyInfo.xcprivacy file
            xmlDoc.Save(privacyInfoPath);
        }

        private void PatchInfoPlist(string buildOutputPath, FGToolsSettings settings)
        {
            if(!settings.SetUsesNonExemptEncryptionFalse) return;

            InfoPlistEditor plistEditor = new InfoPlistEditor(buildOutputPath);
            plistEditor.SetUsesNonExemptEncryption(false);
            plistEditor.WriteToFile();
        }
    }
}

#endif

