using System;
using System.Collections.Generic;
using DG.Tweening;
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
            if (CoreWorld.BuyTheme(theme))
            { 
                CoreWorld.Theme = theme;
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
            if (CoreWorld.HasTheme(buyTheme.theme))
            {
                buyAnim.Animate().Complete();
            }
        }

        protected override bool Get => CoreWorld.Theme == buyTheme.theme;

        protected override bool Set
        {
            set
            {
                if(!value) return;
                if (!CoreWorld.HasTheme(buyTheme.theme))
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
                        };
                    }
                }
                else
                {
                    CoreWorld.Theme = buyTheme.theme;
                }
            }
        }
    }
}