using System;
using TMPro;
using UnityEngine;

namespace FunGames.Core.Utils
{
    [RequireComponent(typeof(TMP_Text))]
    public class FGTMPProText : MonoBehaviour
    {
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (String.IsNullOrEmpty(Application.productName))
            {
                Debug.LogWarning("Game Name is missing in FGMainSettings.");
                return;
            }

            _text.text = _text.text.Replace("THE GAME", Application.productName);
        }
    }
}