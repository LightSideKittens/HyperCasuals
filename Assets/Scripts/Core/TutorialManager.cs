using System.Collections.Generic;
using LSCore;
using LSCore.AnimationsModule;
using LSCore.Async;
using UnityEngine;

namespace Core
{
    public class TutorialManager : SingleService<TutorialManager>
    {
        public Dragger dragger;
        public AnimSequencer pointerAnim;
        public AnimSequencer shapeBlinkAnim;
        public GameObject pointer;
        [SerializeReference] public List<DoIt> onCompleted;
        
        protected override void Init()
        {
            base.Init();
            pointerAnim.Animate();
            shapeBlinkAnim.Animate();
            
            dragger.Started += _ =>
            {
                pointerAnim.Kill();
                pointer.SetActive(false);
            };

            dragger.Ended += _ =>
            {
                pointerAnim.Kill();
                pointer.SetActive(true);
                pointerAnim.Animate();
            };

            FieldManager.Placed += OnPlaced;
        }

        protected override void DeInit()
        {
            base.DeInit();
            FieldManager.Placed -= OnPlaced;
        }

        private void OnPlaced(FieldManager.PlaceData obj)
        {
            FieldManager.DeleteSpawners();
            shapeBlinkAnim.Kill();
            shapeBlinkAnim.Init();
            pointerAnim.Kill();
            pointer.SetActive(false);
            Wait.Delay(1, onCompleted.Do);
        }
    }
}