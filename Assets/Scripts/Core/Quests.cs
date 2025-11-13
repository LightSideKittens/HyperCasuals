using System;
using LSCore;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Core
{
    public class Quests : SingleScriptableObject<Quests>
    {
        [Serializable]
        public abstract class Quest
        {
            public abstract void Init();
            public abstract void DeInit();
        }
        
        public static JObject Config => JTokenGameConfig.Get("Quests");
        
        [SerializeReference] public Quest[] quests;
        public ViewState[] questViews;

        public static ViewState[] Prefabs => Instance.questViews;
        public static Quest[] QuestsHandlers => Instance.quests;
        
        public static JObject CurrentQuest => Config.AsJ<JObject>("quest");
        
        public static int CompletedQuests
        {
            get => Config.As("completedQuests", 0);
            set
            {
                Config["quest"] = new JObject();
                Config["completedQuests"] = value;
            }
        }
        
        public static void Init() => QuestsHandlers.GetCyclic(CompletedQuests).Init();
        public static void DeInit() => QuestsHandlers.GetCyclic(CompletedQuests).DeInit();
    }
    
    [Serializable]
    public class SliderChanger : ViewState.BaseSliderChanger
    {
        protected override string ViewJObjectKey => "slider";
        protected override int ActualValue => Quests.CurrentQuest.As("collectedCount", 0);
        
        protected override int SavedValue
        {
            get => ViewJObject.As("collectedCount", 0);
            set => ViewJObject["collectedCount"] = value;
        }
    }

    [Serializable]
    public class CollectBlocksQuest : Quests.Quest
    {
        [Id] public Id blockId;
        
        public override void Init()
        {
            FieldManager.Placed += OnPlaced;
            Booster.Used += OnGridChanged;
        }

        public override void DeInit()
        {
            FieldManager.Placed -= OnPlaced;
            Booster.Used -= OnGridChanged;
        }
        
        private void OnPlaced(FieldManager.PlaceData data)
        {
            OnGridChanged(data.lastGrid, data.currentGrid);
        }

        private void OnGridChanged(Block[,] lastGrid, Block[,] currentGrid)
        {
            var destroyedBlocksSet = FieldManager.GetDestroyedBlocks(lastGrid, currentGrid);
            int destroyedCount = 0;
            
            foreach (var block in destroyedBlocksSet)
            {
                if (block.id == blockId)
                {
                    destroyedCount++;
                }
            }

            Quests.CurrentQuest.Increase("collectedCount", destroyedCount);
        }
    }
}