using System;
using UnityEngine;

namespace Core
{
    public abstract class Goal : MonoBehaviour
    {
        public event Action Reached;

        private bool isReached;
        public bool IsReached
        {
            get => isReached;
            set
            {
                if (!isReached && value)
                {
                    Reached?.Invoke();
                }
                isReached = value;
            }
        }
    }
}