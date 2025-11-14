using Animatable;
using UnityEngine;

namespace Core
{
    public class CoreAnimatableCanvas : BaseAnimatableCanvas<CoreAnimatableCanvas>
    {
        [SerializeField] private BlockCount blockCount;
        internal static BlockCount BlockCount => Instance.blockCount;
        
        protected override void Init()
        {
            base.Init();
            blockCount.Init();
        }

        protected override void OnClean()
        {
            base.OnClean();
            blockCount.ReleaseAll();
        }
    }
}