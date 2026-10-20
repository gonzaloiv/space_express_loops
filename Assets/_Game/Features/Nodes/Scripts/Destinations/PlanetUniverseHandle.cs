using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class PlanetUniverseHandle : MonoBehaviour
    {
        [SerializeField] private PlanetBehaviour planet;
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private SphereCollider handleCollider;
        [SerializeField] private float minWorldRadius = 0.022f;

        private int interactorId = -1;

        public bool IsSelected { get; private set; }
        public int InteractorId => interactorId;
        public Pose GrabPose => new(grabbable.transform.position, grabbable.transform.rotation);

        private void OnEnable()
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }

        private void OnDisable()
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            switch (pointer.Type)
            {
                case PointerEventType.Select:
                    IsSelected = true;
                    interactorId = pointer.Identifier;
                    break;
                case PointerEventType.Unselect:
                case PointerEventType.Cancel:
                    if (pointer.Identifier != interactorId)
                        return;

                    IsSelected = false;
                    interactorId = -1;
                    ResetProxy();
                    break;
            }
        }

        private void LateUpdate()
        {
            if (!planet.IsActive)
                return;

            float worldScale = transform.lossyScale.x;
            if (worldScale <= 0.0001f)
                return;

            float localPlanetRadius = planet.NodeBody.Radius / worldScale;
            float localMinRadius = minWorldRadius / worldScale;
            handleCollider.radius = Mathf.Max(localPlanetRadius, localMinRadius);
        }

        private void ResetProxy()
        {
            grabbable.transform.localPosition = Vector3.zero;
            grabbable.transform.localRotation = Quaternion.identity;
        }
    }
}
