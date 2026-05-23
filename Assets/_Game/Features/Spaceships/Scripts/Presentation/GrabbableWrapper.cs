using System;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GrabbableWrapper : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private Renderer grabbableRenderer;
        [SerializeField] private GrabZone grabZone;
        [SerializeField] private AudioSource grabAudioSource;

        private bool pointerHandlingEnabled;
        private bool isGrabbed;
        private bool isPickingDestination;

        public Action selected;
        public Action released;

        public void SetInteractionActive(bool active) => grabbable.SetActive(active);

        public void SetWorldPosition(Vector3 position) => transform.position = position;

        public void Show(bool pickingDestination = false)
        {
            isPickingDestination = pickingDestination;
            pointerHandlingEnabled = true;
            grabbable.SetActive(true);
            grabbable.transform.LocalReset();
            grabZone.SetActive(true);
            RefreshBodyVisibility();
        }

        public void Hide()
        {
            pointerHandlingEnabled = false;
            isGrabbed = false;
            isPickingDestination = false;
            grabZone.SetActive(false);
            RefreshBodyVisibility();
        }

        public void EnablePointerHandling() => pointerHandlingEnabled = true;

        public void DisablePointerHandling() => pointerHandlingEnabled = false;

        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (!pointerHandlingEnabled)
                return;

            switch (pointer.Type)
            {
                case PointerEventType.Select:
                    isGrabbed = true;
                    RefreshBodyVisibility();
                    selected?.Invoke();
                    grabAudioSource.Play();
                    break;
                case PointerEventType.Unselect:
                case PointerEventType.Cancel:
                    isGrabbed = false;
                    RefreshBodyVisibility();
                    released?.Invoke();
                    break;
            }
        }

        private void RefreshBodyVisibility()
        {
            bool visible = grabZone.gameObject.activeSelf && !isGrabbed && !isPickingDestination;
            grabbableRenderer.gameObject.SetActive(visible);
        }
    }
}
