using System;
using System.Collections.Generic;
using DG.Tweening;
using Firebase.Analytics;
using LSCore;
using LSCore.AnimationsModule;
using Sirenix.OdinInspector;

namespace Common
{
    [Serializable]
    public class BuyTheme : DoIt
    {
        [ValueDropdown("ThemeIndices")] public int theme;
        
        private IEnumerable<ValueDropdownItem<int>> ThemeIndices
        {
            get
            {
                int index = 0;
                foreach (var se in Themes.List)
                {
                    yield return new ValueDropdownItem<int>(se, index++);
                }
            }
        }
        public override void Do()
        {
            if (GameSave.BuyTheme(theme))
            { 
                GameSave.Theme = theme;
            }
        }
    }
    
    [Serializable]
    public class ThemeToggleData : BaseToggleData
    {
        [Serializable]
        public class ConfirmBuy : DoIt
        {
            public override void Do()
            {
                spend?.Invoke();
            }
        }
        
        public BuyTheme buyTheme;
        public FundByText price;
        public AnimSequencer buyAnim;
        public CreateOrShowCanvasView showConfirmView;
        private static Action spend;
        
        protected override void Init()
        {
            base.Init();
            if (GameSave.HasTheme(buyTheme.theme))
            {
                buyAnim.Animate().Complete();
            }
        }

        protected override bool Get => GameSave.Theme == buyTheme.theme;

        protected override bool Set
        {
            set
            {
                if(!value) return;
                if (!GameSave.HasTheme(buyTheme.theme))
                {
                    if (price.Spend(out spend))
                    {
                        showConfirmView.Do();
                        spend += () =>
                        {
                            buyAnim.Animate();
                            buyTheme.Do();
                            spend = null;
                            IsOn = true;
                            Analytic.LogEvent("buy_theme", "theme", buyTheme.theme.ToString());
                        };
                    }
                }
                else
                {
                    GameSave.Theme = buyTheme.theme;
                    Analytic.LogEvent("select_theme", "theme", buyTheme.theme.ToString());
                }
            }
        }
    }
}