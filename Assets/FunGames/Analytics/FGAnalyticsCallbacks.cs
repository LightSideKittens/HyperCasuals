using System;
using System.Collections.Generic;
using FunGames.Core.Modules;

namespace FunGames.Analytics
{
    public class FGAnalyticsCallbacks : FGModuleCallbacks
    {
        internal Action<LevelStatus, string, int> _ProgressionEvent01;
        internal Action<LevelStatus, string, string, int> _ProgressionEvent02;
        internal Action<LevelStatus, string, string, string, int> _ProgressionEvent03;
        internal Action<string, float> _DesignEventSimple;
        internal Action<string, Dictionary<string, object>, float> _DesignEventDictio;
        internal Action<AdAction, AdType, string, string> _AdEvent;
        internal Action<string, bool> _EventReceived;

        public event Action<LevelStatus, string, int> OnSendProgressionEvent01
        {
            add => _ProgressionEvent01 += value;
            remove => _ProgressionEvent01 -= value;
        }

        public event Action<LevelStatus, string, string, int> OnSendProgressionEvent02
        {
            add => _ProgressionEvent02 += value;
            remove => _ProgressionEvent02 -= value;
        }

        public event Action<LevelStatus, string, string, string, int> OnSendProgressionEvent03
        {
            add => _ProgressionEvent03 += value;
            remove => _ProgressionEvent03 -= value;
        }

        public event Action<string, float> OnSendDesignEventSimple
        {
            add => _DesignEventSimple += value;
            remove => _DesignEventSimple -= value;
        }

        public event Action<string, Dictionary<string, object>, float> OnSendDesignEventDictio
        {
            add => _DesignEventDictio += value;
            remove => _DesignEventDictio -= value;
        }

        public event Action<AdAction, AdType, string, string> OnSendAdEvent
        {
            add => _AdEvent += value;
            remove => _AdEvent -= value;
        }

        public event Action<string, bool> OnEventReceived
        {
            add => _EventReceived += value;
            remove => _EventReceived -= value;
        }

        public override void Clear()
        {
            base.Clear();
            _ProgressionEvent01 = null;
            _ProgressionEvent02 = null;
            _ProgressionEvent03 = null;
            _DesignEventSimple = null;
            _DesignEventDictio = null;
            _AdEvent = null;
        }
    }
}