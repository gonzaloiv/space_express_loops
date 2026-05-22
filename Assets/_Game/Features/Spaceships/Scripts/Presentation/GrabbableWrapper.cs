using System;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;
using DigitalLove.VFX;

namespace DigitalLove.Game.Spaceships
{
    public class GrabbableWrapper : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private Renderer grabbableRenderer;
        [SerializeField] private ConstantRotation constantRotation;
        [SerializeField] private GrabZone grabZone;
        [SerializeField] private AudioSource grabAudioSource;

        private bool isListeningForPointerEvents;

        public Action selected;
        public Action released;

        public void SetInteractionActive(bool active) => grabbable.SetActive(active);

        public void SetWorldPosition(Vector3 position) => transform.position = position;

        public void BeginDestinationSelection()
        {
            EnablePointerHandling();
            grabbable.SetActive(true);
            grabZone.SetActive(true);
        }

        public void Show()
        {
            EnablePointerHandling();
            grabbable.SetActive(true);
            grabbable.transform.LocalReset();
            constantRotation.IsEnabled = true;
            grabZone.SetActive(true);
            grabbableRenderer.gameObject.SetActive(true);
        }

        public void Hide()
        {
            DisablePointerHandling();
            constantRotation.IsEnabled = false;
            grabZone.SetActive(false);
            grabbableRenderer.gameObject.SetActive(false);
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
                    grabbableRenderer.gameObject.SetActive(false);
                    selected?.Invoke();
                    grabAudioSource.Play();
                    break;
                case PointerEventType.Unselect:
                case PointerEventType.Cancel:
                    if (grabZone.gameObject.activeSelf)
                        grabbableRenderer.gameObject.SetActive(true);
                    released?.Invoke();
                    break;
            }
        }
    }
}
