using System.Collections.Generic;
using Firebase.Analytics;
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
        public Transform blinkShape;
        public GameObject pointer;
        [SerializeReference] public List<DoIt> onCompleted;
        private static bool lastFieldSaveEnabled;
        protected override void Init()
        {
            base.Init();
            blinkShape.SetParent(FieldAppearance.Field, false);
            lastFieldSaveEnabled = FieldSave.IsEnabled;
            FieldSave.IsEnabled = false;
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
            FieldSave.IsEnabled = lastFieldSaveEnabled;
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
            Analytic.LogEvent("tutorial_completed", ("level", GameSave.TutorialLevel));
        }
    }
}