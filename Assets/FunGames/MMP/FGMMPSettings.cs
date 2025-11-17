using FunGames.Core.Utils;
using UnityEngine;

namespace FunGames.MMP
{
    [CreateAssetMenu(fileName = FGPath.ASSETS_RESOURCES + PATH, menuName = PATH, order = ORDER)]
    public class FGMMPSettings : FGModuleSettingsAbstract<FGMMPSettings>
    {
        public const string NAME = "FGMMPSettings";
        const string PATH = FGPath.FUNGAMES + "/" + NAME;

        protected override FGMMPSettings LoadResources()
        {
            return Resources.Load<FGMMPSettings>(PATH);
        }
    }
}