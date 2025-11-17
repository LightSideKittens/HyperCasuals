using System;
using FunGames.Core.Modules;

namespace FunGames.MMP
{
    public class FGMMPCallbacks : FGModuleCallbacks
    {
        internal Action<string> _onDeferredDeepLink;
        
        public event Action<string> OnDeferredDeepLink
        {
            add => _onDeferredDeepLink += value;
            remove => _onDeferredDeepLink -= value;
        }
    }
}