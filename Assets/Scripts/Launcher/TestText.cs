using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using FontStyles = TMPro.FontStyles;

namespace Launcher
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class TestText : MonoBehaviour
    {
        public float fontSize;
        public FontAsset font;
        public HorizontalAlignmentOptions options;
        public VerticalAlignmentOptions options2;
        public string text;
        public Color color2;
        public TextWrappingModes textWrappingMode;
        public FontStyles fontStyle;
    }
}