using System.Collections.Generic;
using System.Linq;
using LSCore;

namespace Core
{
    public class GoalWindow : BaseWindow<GoalWindow>
    {
        private List<Goal> goals;
        protected override bool ActiveByDefault => true;
        private int reachedCount;
        
        protected override void Init()
        {
            base.Init();
            goals = GetComponentsInChildren<Goal>().ToList();
            goals.ForEach(goal => goal.Reached += OnReached);
            Manager.OnlyShow();
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