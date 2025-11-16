#if UNITY_EDITOR && UNITY_IOS
using System;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class InfoPlistEditor
{
    private readonly PlistDocument _plist = new PlistDocument();
    private string _plistPath;
    private string _plistContent;

    public InfoPlistEditor(string buildPath)
    {
        _plistPath = Path.Combine(buildPath, "Info.plist");
        _plist.ReadFromString(File.ReadAllText(_plistPath));
        _plistContent = File.ReadAllText(_plistPath);
    }

    public void WriteToFile()
    {
        File.WriteAllText(_plistPath, _plist.WriteToString());
    }

    public void SetUsesNonExemptEncryption(bool usesNonExemptEncryption)
    {
        _plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", usesNonExemptEncryption);
    }

    public void AddSkAdNetworkIdentifier(string id)
    {
        if (_plist == null) return;
        // Get root
        PlistElementDict rootDict = _plist.root;

        // Check if SKAdNetworkItems already exists
        PlistElementArray skAdNetworkItems = null;
        if (rootDict.values.ContainsKey("SKAdNetworkItems"))
        {
            try
            {
                skAdNetworkItems = rootDict.values["SKAdNetworkItems"] as PlistElementArray;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not obtain SKAdNetworkItems PlistElementArray: {e.Message}");
            }
        }

        // If not exists, create it
        if (skAdNetworkItems == null)
        {
            skAdNetworkItems = rootDict.CreateArray("SKAdNetworkItems");
        }

        //Add SKAdNetwork ID
        if (_plistContent!=null && !_plistContent.Contains(id))
        {
            PlistElementDict skAdNetworkIdentifierDict = skAdNetworkItems.AddDict();
            skAdNetworkIdentifierDict.SetString("SKAdNetworkIdentifier", id);
        }
    }
}
#endif