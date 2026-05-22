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

        private bool isListeningForPointerEvents;
        private bool isGrabbed;

        public Action selected;
        public Action released;

        public void SetInteractionActive(bool active) => grabbable.SetActive(active);

        public void SetWorldPosition(Vector3 position) => transform.position = position;

        public void BeginDestinationSelection()
        {
            Show();
            SetInteractionActive(true);
        }

        public void Show()
        {
            EnablePointerHandling();
            grabbable.SetActive(true);
            grabbable.transform.LocalReset();
            grabZone.SetActive(true);
            SetGrabbableBodyVisible(!isGrabbed);
        }

        public void Hide()
        {
            DisablePointerHandling();
            isGrabbed = false;
            grabZone.SetActive(false);
            SetGrabbableBodyVisible(false);
        }

        public void EnablePointerHandling()
        {
            if (isListeningForPointerEvents)
                return;

            isListeningForPointerEvents = true;
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }

        public void DisablePointerHandling()
        {
            if (!isListeningForPointerEvents)
                return;

            isListeningForPointerEvents = false;
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (!isListeningForPointerEvents)
                return;

            switch (pointer.Type)
            {
                case PointerEventType.Select:
                    isGrabbed = true;
                    SetGrabbableBodyVisible(false);
                    selected?.Invoke();
                    grabAudioSource.Play();
                    break;
                case PointerEventType.Unselect:
                case PointerEventType.Cancel:
                    isGrabbed = false;
                    if (grabZone.gameObject.activeSelf)
                        SetGrabbableBodyVisible(true);
                    released?.Invoke();
                    break;
            }
        }

        private void SetGrabbableBodyVisible(bool visible) =>
            grabbableRenderer.gameObject.SetActive(visible);
    }
}
