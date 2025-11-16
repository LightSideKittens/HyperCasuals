using System;
using FunGames.Core.Modules;

namespace FunGames.Core
{
    public class FGCoreCallbacks : FGModuleCallbacks
    {
        internal Action _onAdsRemoved;

        public event Action OnAdsRemoved
        {
            add => _onAdsRemoved += value;
            remove => _onAdsRemoved -= value;
        }

        public override void Clear()
        {
            base.Clear();
            _onAdsRemoved = null;
        }
    }
}