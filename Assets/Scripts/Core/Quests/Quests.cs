using System;
using Animatable;
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
            public abstract ViewState ViewState { get; }
        }
        
        public static JObject Config => JTokenGameConfig.Get("Quests");
        
        [SerializeReference] public Quest[] quests;
        
        public static Quest[] QuestsHandlers => Instance.quests;
        public static Quest CurrentQuestHandler => QuestsHandlers.GetCyclic(CompletedQuests);
        
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
        
        private static Quest currentQuest;
        public static void Init()
        {
            currentQuest = CurrentQuestHandler;
            currentQuest.Init();
        }

        public static void DeInit() => currentQuest.DeInit();
        
        [Serializable]
        public class GetCurrentQuestView : Get<ViewState>
        {
            public override ViewState Data => CurrentQuestHandler.ViewState;
        }
        
        [Serializable]
        public class SliderChanger : ViewState.BaseSliderChanger
        {
            [SerializeReference] public DoIt onCompleted;
            protected override string ViewJObjectKey => "slider";
            protected override int ActualValue => CurrentQuest.As("collectedCount", 0);

            private int questId;
            
            public override void Init()
            {
                base.Init();
                questId = CompletedQuests;
            }

            public override bool CanChange => base.CanChange && questId == CompletedQuests;

            protected override void OnChange(Action onComplete)
            {
                base.OnChange(OnComplete);

                void OnComplete()
                {
                    if (ActualValue >= sliderAnim.FirstTarget.maxValue)
                    {
                        CompletedQuests++;
                        onCompleted.Do();
                        Analytic.LogEvent("quest_completed", ("quest", CompletedQuests));
                    }
                    onComplete?.Invoke();
                }
            }

            protected override int SavedValue
            {
                get => ViewJObject.As("collectedCount", 0);
                set => ViewJObject["collectedCount"] = value;
            }
        }
        
        [Serializable]
        public class CollectBlocksQuestSwitcher : ViewState.Switcher
        {
            private int questId;
            public override void Init()
            {
                base.Init();
                questId = CompletedQuests;
            }
            
            protected override string ViewJObjectKey => "questState";

            protected override string CurrentState
            {
                get
                {
                    if(questId == CompletedQuests) return "unCompleted";
                    return "completed";
                }
            }
            protected override string DefaultState => "unCompleted";
        }

        [Serializable]
        public class CollectBlocksQuest : Quest
        {
            public QuestView view;
            public static int collectedCount;
            
            public override void Init()
            {
                FieldManager.Placed += OnPlaced;
                Booster.Used += OnGridChanged;
                WinWindow.Showing += OnWin;
                collectedCount = CurrentQuest.As("collectedCount", 0);
            }

            private void OnWin()
            {
                CurrentQuest.Increase("collectedCount", collectedCount);
            }

            public override void DeInit()
            {
                WinWindow.Showing -= OnWin;
                FieldManager.Placed -= OnPlaced;
                Booster.Used -= OnGridChanged;
            }

            public override ViewState ViewState => view;

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
                    if (block.id == view.data.id)
                    {
                        destroyedCount++;
                        BlockCount.Create(1, block.sprite, block.transform);
                    }
                }

                collectedCount += destroyedCount;
            }
        }
    }
}