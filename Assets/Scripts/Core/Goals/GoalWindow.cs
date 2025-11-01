using System.Collections.Generic;
using System.Linq;
using LSCore;
using UnityEngine;

namespace Core
{
    public class GoalWindow : BaseWindow<GoalWindow>
    {
        private List<Goal> goals;
        protected override bool ActiveByDefault => true;
        private int reachedCount;
        private static float startTime;
        public static float LevelTime => Time.realtimeSinceStartup - startTime;
        public static Analytic.Param LevelTimeParam => ("time", (int)LevelTime);
        
        protected override void Init()
        {
            base.Init();

            goals = GetComponentsInChildren<Goal>().ToList();
            goals.ForEach(goal => goal.Reached += OnReached);
            Manager.OnlyShow();
            startTime = Time.realtimeSinceStartup;
        }

        private void OnReached()
        {
            reachedCount++;
            if (reachedCount >= goals.Count)
            {
                WinWindow.Show();
            }
        }
    }
}