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

        public Transform Transform => grabbable.transform;

        public void SubscribePointerEvents(Action<PointerEvent> handler) =>
            grabbable.WhenPointerEventRaised += handler;

        public void UnsubscribePointerEvents(Action<PointerEvent> handler) =>
            grabbable.WhenPointerEventRaised -= handler;

        public void SetInteractionActive(bool active) => grabbable.SetActive(active);

        public void SetWorldPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void Show()
        {
            grabbable.SetActive(false);
            grabbable.transform.LocalReset();
            grabbableRenderer.gameObject.SetActive(true);
            grabbable.SetActive(true);
            constantRotation.IsEnabled = true;
            grabZone.gameObject.SetActive(true);
        }

        public void Hide()
        {
            grabbableRenderer.gameObject.SetActive(false);
            constantRotation.IsEnabled = false;
            grabZone.gameObject.SetActive(false);
        }
    }
}
