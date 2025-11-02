using System;
using System.Collections.Generic;
using System.Linq;
using LSCore;
using LSCore.Async;
using LSCore.Extensions;
using UnityEngine;

namespace Core
{
    public class GoalWindow : BaseWindow<GoalWindow>
    {
        public LSImage chest;
        public LSText chestInfo;
        
        private List<Goal> goals;
        protected override bool ActiveByDefault => true;
        private int reachedCount;
        private static float startTime;
        public static float LevelTime => Time.realtimeSinceStartup - startTime;
        public static Analytic.Param LevelTimeParam => ("time", (int)LevelTime);

        private Vector2Int? chestIndex;
        private GameObject chestGo;
        private bool CanPlaceChest => GameSave.CollectedChests < Chests.ChestCountForIssue;
        
        protected override void Init()
        {
            base.Init();

            goals = GetComponentsInChildren<Goal>().ToList();
            goals.ForEach(goal => goal.Reached += OnReached);
            Manager.OnlyShow();
            startTime = Time.realtimeSinceStartup;

            FieldManager.Started += OnStarted;
        }
        
        protected override void DeInit()
        {
            base.DeInit();
            Booster.Used -= OnBoosterUsed;
            FieldManager.Placed -= PlaceChest;
            FieldManager.Started -= OnStarted;
        }
        
        private void OnStarted()
        {
            SetupChestView();
            
            if (CanPlaceChest && !GameSave.IsChestGot)
            {
                FieldManager.Placed += PlaceChest;
                Booster.Used += OnBoosterUsed;
                PlaceChest();
            }
        }

        private void OnBoosterUsed(Block[,] lastGrid, Block[,] newGrid)
        {
            TryGotChest();
        }
        
        private void PlaceChest(FieldManager.PlaceData _)
        {
            if (chestIndex == null)
            { 
                PlaceChest();
            }
            else
            {
                TryGotChest();
            }
        }

        private void TryGotChest()
        {
            if(chestIndex is null) return;
            
            if(FieldManager.Grid.Get(chestIndex.Value) == null)
            {
                OnChestGot();
            }
        }
        
        private void PlaceChest()
        {
            /*var block = FieldManager.ActiveBlocks.RandomElement();
            if (block != null)
            {
                var index = FieldManager.ToIndex(block.transform.position);
                chestIndex = index;
                chestGo = Instantiate(Chests.Current.block, FieldManager.ToPos(index), Quaternion.identity);
            }*/
        }

        private void OnChestGot()
        {
            GameSave.IsChestGot = true;
            GameSave.CollectedChests++;
            SetupChestView();
            Destroy(chestGo);
            FieldManager.Placed -= PlaceChest;
        }

        private void OnReached()
        {
            reachedCount++;
            if (reachedCount >= goals.Count)
            {
                Wait.EndOfFrame(WinWindow.Show);
            }
        }

        private void SetupChestView()
        {
            chest.sprite = Chests.Current.ChestSprite;

            var nowTicks = DateTime.UtcNow.Ticks;
            
            if (CanPlaceChest)
            {
                chestInfo.text = $"{GameSave.CollectedChests}/{Chests.ChestCountForIssue}";
            }
            else if(GameSave.RenewalDateTime <= nowTicks)
            {
                GameSave.RenewalDateTime = nowTicks + Chests.RenewalTime;
            }

            var diff = GameSave.RenewalDateTime - nowTicks;
            if (diff > 0)
            {
                TimeSpan.FromTicks(diff).Seconder(time =>
                {
                    chestInfo.text = time.Timelyze();
                }).KillOnDestroy(chestInfo);
            }
        }
    }
}