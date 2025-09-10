using System;
using LSCore;
using LSCore.Attributes;
using UnityEngine;

namespace Core
{
    public class Chests : SingleScriptableObject<Chests>
    {
        [Serializable]
        public class Data
        {
            public GameObject block;
            public GameObject chestView;
            public UIView rewardView;
        }

        public int chestCountForIssue = 5;
        [TimeSpan(12, 0, 0, options = TimeAttribute.Options.WatchTime)] 
        public long renewalTime;
        
        public Data[] chests;
        public static Data[] List => Instance.chests;
        public static Data Current => List.GetWrapped(GameSave.Level - 1, 10);
        public static int ChestCountForIssue => Instance.chestCountForIssue;
        
        [Serializable]
        public class GetCurrent : Get<Data>
        {
            public override Data Data => Current;
        }
    }
}