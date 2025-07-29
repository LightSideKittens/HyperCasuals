using UnityEngine;

namespace Core
{
    public abstract class Goal : MonoBehaviour
    {
        public abstract bool IsReached { get; }
    }
}