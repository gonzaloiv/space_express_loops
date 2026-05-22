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
        [SerializeField] private Transform grabZone;
        [SerializeField] private AudioSource grabAudioSource;

        private bool isPointerHandlingEnabled;

        public Action selected;
        public Action released;

        public Transform Transform => grabbable.transform;

        public void SetInteractionActive(bool active) => grabbable.SetActive(active);

        public void SetWorldPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void EnablePointerHandling()
        {
            if (isPointerHandlingEnabled)
                return;

            isPointerHandlingEnabled = true;
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }

        public void DisablePointerHandling()
        {
            if (!isPointerHandlingEnabled)
                return;

            isPointerHandlingEnabled = false;
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }

        public void Show()
        {
            EnablePointerHandling();
            grabbable.SetActive(true);
            grabbable.transform.LocalReset();
            constantRotation.IsEnabled = true;
            grabZone.gameObject.SetActive(true);
            SetGrabbableRendererVisible(true);
        }

        public void Hide()
        {
            DisablePointerHandling();
            SetGrabbableRendererVisible(false);
            constantRotation.IsEnabled = false;
            grabZone.gameObject.SetActive(false);
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (!isPointerHandlingEnabled)
                return;

            switch (pointer.Type)
            {
                case PointerEventType.Select:
                    SetGrabbableRendererVisible(false);
                    selected?.Invoke();
                    grabAudioSource.Play();
                    break;
                case PointerEventType.Unselect:
                case PointerEventType.Cancel:
                    SetGrabbableRendererVisible(true);
                    released?.Invoke();
                    break;
            }
        }

        private void SetGrabbableRendererVisible(bool visible)
        {
            grabbableRenderer.gameObject.SetActive(visible);
        }
    }
}
