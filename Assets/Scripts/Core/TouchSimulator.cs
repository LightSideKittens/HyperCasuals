using System;
using LSCore;
using LSCore.AnimationsModule;
using UnityEngine;

namespace Core
{
    public class TouchSimulator : MonoBehaviour
    {
        [Serializable]
        public class TouchDown : DoIt
        {
            public override void Do()
            {
                LSInput.Simulator.TouchDown(GetPosition());
            }
        }
        
        [Serializable]
        public class TouchUp : DoIt
        {
            public override void Do()
            {
                LSInput.Simulator.TouchUp(0);
            }
        }
        
        public AnimSequencer anim;
        public Transform touchPoint;
        public static Transform TouchPoint;
        public static Vector3 GetPosition() => Camera.main.WorldToScreenPoint(TouchPoint.position);
        
        private void Awake()
        {
            TouchPoint = touchPoint;
            LSInput.IsManualControl = true;
        }

        private void Start()
        {
            anim.Animate();
        }

        private void Update()
        {
            if (LSInput.TouchCount > 0)
            {
                LSInput.Simulator.TouchMove(0, GetPosition());
            }
        }
    }
}