using FunGames.Core;
using FunGames.Core.Modules;

namespace FunGames.MMP
{
    public class FGMMPManager : FGModuleAbstract<FGMMPManager, FGMMPCallbacks, FGMMPSettings>
    {
        public override FGMMPSettings Settings => FGMMPSettings.settings;
        protected override FGModule Parent => FGCore.Instance;
        protected override string EventName => "MMP";
        protected override string RemoteConfigKey => "FGMMP";

        protected override void InitializeCallbacks()
        {
            FunGamesSDK.Callbacks.Initialization += Initialize;
        }

        protected override void OnAwake()
        {
        }

        protected override void OnStart()
        {
        }

        protected override void InitializeModule()
        {
           InitializationComplete(true);
        }

        protected override void ClearInitialization()
        {
        }
    }
}