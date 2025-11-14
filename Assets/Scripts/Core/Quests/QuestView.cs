using LSCore;

namespace Core
{
    public class QuestView : ViewState
    {
        public FieldAppearance.BlockData data;
        public LSImage icon;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetupIcon();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(FieldAppearance.IsNull || icon == null) return;
            SetupIcon();
        }
#endif

        public void SetupIcon()
        {
            icon.sprite = data.GetBlock(Levels.FieldAppearance[GameSave.Theme]).sprite;
        }
    }
}