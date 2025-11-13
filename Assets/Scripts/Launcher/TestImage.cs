using UnityEngine;

namespace Launcher
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class TestImage : MonoBehaviour
    {
        public string test = "test";
        public Color color;
        public int size;
        public float pixelsPerunity;
        public Sprite sprite;
    }
}