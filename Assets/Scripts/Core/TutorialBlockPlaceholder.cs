using LSCore;

namespace Core
{
    public class TutorialBlockPlaceholder : BlockPlaceholder
    {
        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            if(World.IsEditMode) return;
#endif
            block.render.enabled = false;
        }
    }
}