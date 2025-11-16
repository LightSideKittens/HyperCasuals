using System;

namespace FunGames.Core.Modules
{
    public class FGModuleCallbacks
    {
        internal Action _onStart;
        internal Action _onInitialization;
        internal Action<bool> _onInitialized;
        
        public event Action Initialization
        {
            add => _onInitialization += value;
            remove => _onInitialization -= value;
        }

        public event Action<bool> OnInitialized
        {
            add => _onInitialized += value;
            remove => _onInitialized -= value;
        }
        
        public event Action OnStart
        {
            add => _onStart += value;
            remove => _onStart -= value;
        }

        public virtual void Clear()
        {
            _onInitialization = null;
            _onInitialized = null;
            _onStart = null;
        }
    }
}