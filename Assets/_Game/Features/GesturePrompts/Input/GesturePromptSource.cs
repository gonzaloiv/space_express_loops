using System;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
{
    public abstract class GesturePromptSource : MonoBehaviour
    {
        public abstract event Action<GestureKind> Completed;
    }
}