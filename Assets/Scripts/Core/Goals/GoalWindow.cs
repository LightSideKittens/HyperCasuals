using System.Collections.Generic;
using System.Linq;
using LSCore;

namespace Core
{
    public class GoalWindow : BaseWindow<GoalWindow>
    {
        private List<Goal> goals;
        protected override bool ActiveByDefault => true;

        protected override void Init()
        {
            base.Init();
            goals = GetComponentsInChildren<Goal>().ToList();
            Manager.OnlyShow();
        }
    }
}