using DG.Tweening;
using LSCore.AnimationsModule;
using TMPro;
using UnityEngine;

public class BonusBlock : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    public AnimSequencer appearAnim;
    public AnimSequencer changeAnim;
    public AnimSequencer destroyAnim;
    
    public int Value
    {
        get => int.Parse(text.text);
        set
        {
            if (value > 1)
            {
                changeAnim.Animate();
                text.text = value.ToString();
            }
            else
            {
                destroyAnim.Animate().OnComplete(() => Destroy(gameObject));
                text.text = "1";
            }
        }
    }
}