using System.Collections.Generic;

namespace FunGames.Core.Modules
{
    public interface FGModule
    {
        public FGModuleInfo ModuleInfo { get; }
        public bool IsInitialized { get; }
        public bool IsChildCompleted { get; }
        public InitializationStatus InitializationStatus { get; }
        public List<FGModule> SubModules { get; }
        public float TotalSubModules { get; }
        public float TotalSubModulesCompleted { get; }
        void Awake();
        void Start();
        public void Log(params string[] message);
        public void LogError(params string[] message);
        public void LogWarning(params string[] message);
        public bool MustBeInitialized();
        public void CheckModuleInitialization();
        public void Initialize();
        public void AddChild(FGModule child);
        public void InitWithoutTimer();
        public void SetMaxInitTime(float time);
        public void SetManualInit();
        public void UnlockManualInit();
    }

    public enum InitializationStatus
    {
        NOT_INITIALIZED,
        IN_PROGRESS,
        COMPLETED
    }
}