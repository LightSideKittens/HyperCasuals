using System;
using System.Collections.Generic;
using System.Text;
using FunGames.Core.Modules;
using FunGames.Core.Settings;
using FunGames.Tools.Utils;

namespace FunGames.Analytics
{
    public abstract class FGAnalyticsAbstract<M, C, S> : FGModuleAbstract<M, C, S>
        where M : FGModuleAbstract<M, C, S>
        where C : FGAnalyticsCallbacks, new()
        where S : IFGModuleSettings
    {
        // protected override IFGModuleParent Parent => FGAnalyticsManager.Instance;

        public List<Action> PoolEvents = new List<Action>();

        protected abstract void Init();

        protected override void InitializeCallbacks()
        {
            FGAnalyticsManager.Instance.Callbacks.Initialization += Initialize;
            FGAnalyticsManager.Instance.Callbacks.OnSendProgressionEvent01 += SendProgressionEvent;
            FGAnalyticsManager.Instance.Callbacks.OnSendProgressionEvent02 += SendProgressionEvent;
            FGAnalyticsManager.Instance.Callbacks.OnSendProgressionEvent03 += SendProgressionEvent;
            FGAnalyticsManager.Instance.Callbacks.OnSendDesignEventSimple += SendDesignEventSimple;
            FGAnalyticsManager.Instance.Callbacks.OnSendDesignEventDictio += SendDesignEventDictio;
            FGAnalyticsManager.Instance.Callbacks.OnSendAdEvent += SendAdEvent;
            Callbacks.OnInitialized += SendAllPoolEvents;
        }

        protected override void InitializeModule()
        {
            Init();
        }

        protected abstract void ProgressionEvent(LevelStatus levelStatus, string prog01,
            int score = FGAnalytics.NO_SCORE);

        protected abstract void ProgressionEvent(LevelStatus levelStatus, string prog01, string prog02,
            int score = FGAnalytics.NO_SCORE);

        protected abstract void ProgressionEvent(LevelStatus levelStatus, string prog01, string prog02, string prog03,
            int score = FGAnalytics.NO_SCORE);

        protected abstract void DesignEventSimple(string eventId, float eventValue);

        protected abstract void DesignEventDictio(string eventId, Dictionary<string, object> customFields);

        protected abstract void AdEvent(AdAction adAction, AdType adType, string adSdkName, string adPlacement);


        public void SendProgressionEvent(LevelStatus levelStatus, string prog01, int score = FGAnalytics.NO_SCORE)
        {
            string eventContent = "{ " + levelStatus + " ; " + prog01 + " ; " + score + "}";
            if (CurrentPlatform.Is(Platform.Editor))
            {
                LogWarning("Progression Event not sent in Editor: " + eventContent);
                return;
            }

            if (!IsInitialized)
            {
                PoolEvents.Add(delegate { SendProgressionEvent(levelStatus, prog01, score); });
                return;
            }

            Log("Progression Event triggered : " + eventContent);
            ProgressionEvent(levelStatus, prog01, score);
            Callbacks._ProgressionEvent01?.Invoke(levelStatus, prog01, score);
        }

        public void SendProgressionEvent(LevelStatus levelStatus, string prog01, string prog02,
            int score = FGAnalytics.NO_SCORE)
        {
            string eventContent = "{ " + levelStatus + " ; " + prog01 + " ; " + prog02 + " ; " + score + "}";
            if (CurrentPlatform.Is(Platform.Editor))
            {
                LogWarning("Progression Event not sent in Editor: " + eventContent);
                return;
            }

            if (!IsInitialized)
            {
                PoolEvents.Add(delegate { SendProgressionEvent(levelStatus, prog01, prog02, score); });
                return;
            }

            Log("Progression Event triggered : " + eventContent);
            ProgressionEvent(levelStatus, prog01, prog02, score);
            Callbacks._ProgressionEvent02?.Invoke(levelStatus, prog01, prog02, score);
        }

        public void SendProgressionEvent(LevelStatus levelStatus, string prog01, string prog02, string prog03,
            int score = FGAnalytics.NO_SCORE)
        {
            string eventContent = "{ " + levelStatus + " ; " + prog01 + " ; " + prog02 + " ; " + prog03 + " ; " +
                                  score + "}";
            if (CurrentPlatform.Is(Platform.Editor))
            {
                LogWarning("Progression Event not sent in Editor: " + eventContent);
                return;
            }

            if (!IsInitialized)
            {
                PoolEvents.Add(delegate { SendProgressionEvent(levelStatus, prog01, prog02, prog03, score); });
                return;
            }

            Log("Progression Event triggered : " + eventContent);
            ProgressionEvent(levelStatus, prog01, prog02, prog03, score);
            Callbacks._ProgressionEvent03?.Invoke(levelStatus, prog01, prog02, prog03, score);
        }

        public void SendDesignEventSimple(string eventId, float eventValue)
        {
            string eventContent = "{" + eventId + " ; " + eventValue + "}";
            if (CurrentPlatform.Is(Platform.Editor))
            {
                LogWarning("Design Event not sent in Editor: " + eventContent);
                return;
            }

            if (!IsInitialized)
            {
                PoolEvents.Add(delegate { SendDesignEventSimple(eventId, eventValue); });
                // LogWarning("Simple Design Event added to pool : module has not been initialized !\n" + eventContent);
                return;
            }

            Log("Design Event triggered : " + eventContent);
            DesignEventSimple(eventId, eventValue);
            Callbacks._DesignEventSimple?.Invoke(eventId, eventValue);
        }

        public void SendDesignEventDictio(string eventId, Dictionary<string, object> customFields, float eventValue = 0)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, object> keyValuePair in customFields)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append(keyValuePair.Key);
                stringBuilder.Append(" : ");
                stringBuilder.Append(keyValuePair.Value);
                stringBuilder.Append(" ;");
            }

            string eventContent = eventId + " | value = " + eventValue + stringBuilder;
            if (CurrentPlatform.Is(Platform.Editor))
            {
                LogWarning("Design Event not sent in Editor: " + eventContent);
                return;
            }

            if (!IsInitialized)
            {
                PoolEvents.Add(delegate { SendDesignEventDictio(eventId, customFields, eventValue); });
                // LogWarning("Dictio Design Event added to pool : module has not been initialized !\n" + eventContent);
                return;
            }

            Log("Design Event triggered : " + eventContent);
            DesignEventDictio(eventId, customFields);
            Callbacks?._DesignEventDictio?.Invoke(eventId, customFields, eventValue);
        }

        public void SendAdEvent(AdAction adAction, AdType adType, string adSdkName, string adPlacement)
        {
            string eventContent = "{" + adAction + " ; " + adType + " ; " + adSdkName + " ; " + adPlacement + "}";
            if (CurrentPlatform.Is(Platform.Editor))
            {
                LogWarning("Ad Event not sent in Editor: " + eventContent);
                return;
            }

            if (!IsInitialized)
            {
                PoolEvents.Add(delegate { SendAdEvent(adAction, adType, adSdkName, adPlacement); });
                // LogWarning("Ad Event added to pool : module has not been initialized !\n" + eventContent);
                return;
            }

            Log("Ad Event triggered : " + eventContent);
            AdEvent(adAction, adType, adSdkName, adPlacement);
            Callbacks?._AdEvent?.Invoke(adAction, adType, adSdkName, adPlacement);
        }

        protected void EventReceived(string eventName, bool eventReceived)
        {
            Callbacks?._EventReceived?.Invoke(eventName, eventReceived);
            // Callbacks._EventReceived = null;
        }

        private void SendAllPoolEvents(bool moduleInitialized)
        {
            if (!moduleInitialized) return;

            Log("Sending all pool events");
            foreach (var progressionEvent in PoolEvents)
            {
                progressionEvent?.Invoke();
            }

            PoolEvents.Clear();
        }

        protected override void ClearInitialization()
        {
            FGAnalyticsManager.Instance.Callbacks.Initialization -= Initialize;
            FGAnalyticsManager.Instance.Callbacks.OnSendProgressionEvent01 -= SendProgressionEvent;
            FGAnalyticsManager.Instance.Callbacks.OnSendProgressionEvent02 -= SendProgressionEvent;
            FGAnalyticsManager.Instance.Callbacks.OnSendProgressionEvent03 -= SendProgressionEvent;
            FGAnalyticsManager.Instance.Callbacks.OnSendDesignEventSimple -= SendDesignEventSimple;
            FGAnalyticsManager.Instance.Callbacks.OnSendDesignEventDictio -= SendDesignEventDictio;
            FGAnalyticsManager.Instance.Callbacks.OnSendAdEvent -= SendAdEvent;
            Callbacks.OnInitialized -= SendAllPoolEvents;
        }
    }
}