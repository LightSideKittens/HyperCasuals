using System;
using System.IO;
using System.Text;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;

namespace FunGames.Core.Editor.Config
{
    public class FGConfigFile
    {
        private static string DefaultFileName => "fg_sdks_config";

        public static void Export()
        {
            string filePath = 
                EditorUtility.SaveFilePanel("Export FG Config", "", DefaultFileName,"csv");
            if (String.IsNullOrEmpty(filePath)) return;
            ExportTo(filePath);
        }
        
        public static void ExportToRoot()
        {
            string filePath = Path.Combine(Application.dataPath, "../", DefaultFileName + ".csv");
            ExportTo(filePath);
        }
        
        private static void ExportTo(string filePath)
        {
            File.WriteAllText(filePath, BuildConfigText());
        }

        private static string BuildConfigText()
        {
            string separator = ",";
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Application.productName.Replace(",", "")},{Application.version}\n");
            sb.Append($"UnityEditor,{Application.unityVersion}\n");
            
            foreach (FGPackage package in ProjectUtils.GetEnumerableOfType<FGPackage>())
            {
                sb.Append(package.ModuleInfo.Id + separator + package.ModuleInfo.Version + "\n");
            }

            sb.Append(FGExternalConfigFile.BuildConfigText());
            return sb.ToString();
        }
    }
}