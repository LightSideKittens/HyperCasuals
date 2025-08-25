namespace Core
{
    public class TutorialBlockPlaceholder : BlockPlaceholder
    {
        protected override void OnPlacing()
        {
            base.OnPlacing();
            block.render.enabled = false;
        }
    }
}