using System;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunSlotTouch : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;

        public event Action Touched;

        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        private void OnPointerEvent(PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Select)
                Touched?.Invoke();
        }
    }
}
