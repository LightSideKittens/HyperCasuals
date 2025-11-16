using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FunGames.Core.Settings;
using FunGames.Core.Utils;
// using FunGames.RemoteConfig;
using FunGames.Tools.Debugging;
using UnityEngine;

namespace FunGames.Core.Modules
{
    /// <summary>
    /// Abstract class for all FunGames modules
    /// </summary>
    /// <typeparam name="M"></typeparam> Module class
    /// <typeparam name="C"></typeparam> Module's callbacks class
    /// <typeparam name="S"></typeparam> Module's settings class
    [DefaultExecutionOrder(-42)]
    public abstract class FGModuleAbstract<M, C, S> : FGSingleton<M>, FGModule
        where M : FGModuleAbstract<M, C, S>
        where C : FGModuleCallbacks, new()
        where S : IFGModuleSettings
    {
        public InitializationStatus InitializationStatus => _initializationStatus;
        public FGModuleInfo ModuleInfo => Settings.ModuleInfo;
        public C Callbacks => _callbacks;
        public abstract S Settings { get; }
        protected abstract FGModule Parent { get; }
        public List<FGModule> SubModules => _children;
        protected abstract string EventName { get; }
        protected abstract string RemoteConfigKey { get; }

        protected abstract void InitializeCallbacks();
        protected abstract void OnAwake();
        protected abstract void OnStart();
        protected abstract void InitializeModule();
        protected abstract void ClearInitialization();

        private C _callbacks = new C();
        private readonly List<FGModule> _children = new List<FGModule>();
        private bool _isSelfInitialized = false;
        private float _timer = 0;
        private float _totalInitTime = 0;
        private IEnumerator _checkInitCoroutine;
        private bool _useCheckInit = true;
        private float _maxInitTime = 5;
        private bool _initLock = false;
        private bool _initAlreadySucceeded = false;
        private bool _initAlreadyFailed = false;
        private InitializationStatus _initializationStatus = InitializationStatus.NOT_INITIALIZED;
        private InitializationStatus _selfInitializationStatus = InitializationStatus.NOT_INITIALIZED;

        private readonly List<FGModule> _recursiveChildrenInitializationStatus =
            new List<FGModule>();

        public const string EVENT_INITIALISATION_START = "InitialisationStart";
        public const string EVENT_INITIALISATION_COMPLETE = "InitialisationComplete";
        private const string EVENT_PARAM_VERSION = "version";
        private const string EVENT_PARAM_SUCCESS = "success";
        private const string EVENT_PARAM_INIT_TIME = "initTime";

        public bool IsInitialized
        {
            get
            {
                bool isInitialized = true;
                foreach (var child in _children)
                {
                    isInitialized &= child.IsInitialized;
                }

                isInitialized &= _isSelfInitialized;
                return isInitialized;
            }
        }

        public float TotalSubModules => _recursiveChildrenInitializationStatus.Count;

        public float TotalSubModulesCompleted
        {
            get
            {
                int i = 0;
                foreach (var subModule in _recursiveChildrenInitializationStatus)
                {
                    if (subModule.InitializationStatus == InitializationStatus.COMPLETED) i++;
                }

                return i;
            }
        }

        public bool IsChildCompleted
        {
            get
            {
                foreach (var child in _children)
                {
                    if (child.InitializationStatus != InitializationStatus.COMPLETED) return false;
                }

                return true;
            }
        }

        public void Awake()
        {
            if (_initLock) return;
            if (Settings == null)
            {
                LogError(EventName + "Settings is null !");
                return;
            }

            try
            {
                Parent?.AddChild(this);
                InitializeCallbacks();
                // FGRemoteConfig.AddDefaultValue(RemoteConfigKey, 1);
                OnAwake();
                Log("Version : " + Settings.ModuleInfo.Version);
            }
            catch (Exception e)
            {
                LogCritical("An exception was raised during Awake :" + e.Message + "\n" + e.StackTrace);
            }
        }

        public void Start()
        {
            try
            {
                if (_initLock) return;
                MapRecursiveChildren(this);
                OnStart();
            }
            catch (Exception e)
            {
                LogCritical("An exception was raised during Start :" + e.Message + "\n" + e.StackTrace);
            }
        }

        public void Initialize()
        {
            if (_initLock) return;
            try
            {
                if (IsInitialized)
                {
                    LogWarning("Module already initialized !");
                    return;
                }

                Log("[" + ModuleInfo.Version + "] Initialization started...");

                if (!MustBeInitialized())
                {
                    // LogWarning("Module disabled ! (Remote =" + FGRemoteConfig.GetBooleanValue(RemoteConfigKey) + ")");
                    Clear();
                    InitializationComplete(true);
                    return;
                }

                _initializationStatus = InitializationStatus.IN_PROGRESS;
                _timer = Time.time;

                if (_useCheckInit)
                {
                    _checkInitCoroutine = CheckInitialization();
                    StartCoroutine(_checkInitCoroutine);
                }

                InitializeModule();
                Callbacks._onInitialization?.Invoke();
            }
            catch (Exception e)
            {
                LogCritical("An exception was raised during init flow :" + e.Message + "\n" + e.StackTrace);
                InitializationComplete(false);
            }
        }

        public void SetManualInit()
        {
            Log("Manual initialization is enabled !");
            Parent?.InitWithoutTimer();
            foreach (var childModule in _children) childModule.SetManualInit();
            _initLock = true;
        }

        public void UnlockManualInit()
        {
            Log("Manual initialization unlocked !");
            _initLock = false;
        }

        public virtual bool MustBeInitialized()
        {
            bool mustBeInitialized = true;
            // mustBeInitialized &= FGRemoteConfig.GetBooleanValue(RemoteConfigKey);
            return mustBeInitialized;
        }

        public void CheckModuleInitialization()
        {
            if (!IsChildCompleted) return;
            if (IsInitialized && _initAlreadySucceeded) return;
            if (!IsInitialized && _initAlreadyFailed) return;
            if (_selfInitializationStatus != InitializationStatus.COMPLETED) return;
            
            _totalInitTime = Time.time - _timer;
            if (_checkInitCoroutine != null) StopCoroutine(_checkInitCoroutine);

            if (IsInitialized)
            {
                Log("...initialization succeeded after " + _totalInitTime + " secs !");
                _initAlreadySucceeded = true;
            }
            else
            {
                LogError("...initialization failed after " + _totalInitTime + " secs !");

                foreach (var child in _children)
                {
                    LogError(child.ModuleInfo.Name + " is init : " + child.IsInitialized);
                }

                LogError("Is self init : " + _isSelfInitialized);

                _initAlreadyFailed = true;
            }

            _initializationStatus = InitializationStatus.COMPLETED;

            Callbacks._onInitialized?.Invoke(IsInitialized);
            Parent?.CheckModuleInitialization();
        }

        protected void InitializationComplete(bool success)
        {
            if (IsInitialized)
            {
                LogWarning("End of initialization has already been triggered !");
                return;
            }

            _isSelfInitialized = success;
            _selfInitializationStatus = InitializationStatus.COMPLETED;
            CheckModuleInitialization();
        }

        public void AddChild(FGModule child)
        {
            _children.Add(child);
        }

        public void Log(params string[] message)
        {
            if (!MustShowLogs()) return;
            if (message.ToString().Contains(EVENT_INITIALISATION_START) ||
                message.ToString().Contains(EVENT_INITIALISATION_COMPLETE)) return;
            FGDebug.Log(FormatLog(message), Settings.Color);
        }

        public void Log(string message, LogLevel logLevel)
        {
            if (!MustShowLogs()) return;
            // if (LogLevel.Debug.Equals(logLevel) && !LogLevel.Debug.Equals(FGCore.Instance.Settings.LogLevel)) return;
            if (message.ToString().Contains(EVENT_INITIALISATION_START) ||
                message.ToString().Contains(EVENT_INITIALISATION_COMPLETE)) return;
            FGDebug.Log(FormatLog(new[] { message }), Settings.Color);
        }

        public void LogError(params string[] message)
        {
            if (!MustShowLogs()) return;
            FGDebug.Log(FormatLog(message), Color.red);
        }

        public void LogWarning(params string[] message)
        {
            if (!MustShowLogs()) return;
            Debug.LogWarning(FormatLog(message));
        }

        public void LogCritical(params string[] message)
        {
            Debug.LogError(FormatLog(message));
        }

        private string FormatLog(string[] messages)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < messages.Length; i++)
            {
                sb.Append(messages[i]);
                if (i < messages.Length - 1) sb.Append("\n");
            }

            return "[FG " + EventName.Trim() + "] " + sb.ToString();
        }

        private IEnumerator CheckInitialization()
        {
            float counter = 0;
            float iteration = 0.2f;
            float maxWaitingDelay = _maxInitTime;
            while (counter <= maxWaitingDelay && !IsInitialized)
            {
                yield return new WaitForSeconds(iteration);
                counter += iteration;
            }

            InitializationComplete(IsInitialized);
        }

        public void InitWithoutTimer()
        {
            _useCheckInit = false;
        }

        public void SetMaxInitTime(float time)
        {
            _maxInitTime = time;
        }

        private void MapRecursiveChildren(FGModule module)
        {
            foreach (var child in module.SubModules)
            {
                if (!_recursiveChildrenInitializationStatus.Contains(child))
                {
                    _recursiveChildrenInitializationStatus.Add(child);
                }

                MapRecursiveChildren(child);
            }
        }

        private bool MustShowLogs()
        {
            // if (FGRemoteConfig.GetIntValue(FGCore.RC_LOG_LEVEL) != 0) return true;
            if (!Settings.LogEnabled) return false;
            if (LogLevel.None.Equals(FGCore.Instance.Settings.LogLevel)) return false;
            return true;
        }

        public void Clear()
        {
            ClearInitialization();
            Callbacks.Clear();
            _isSelfInitialized = false;
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}