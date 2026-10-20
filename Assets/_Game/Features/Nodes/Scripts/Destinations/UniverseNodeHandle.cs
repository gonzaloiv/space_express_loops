using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class UniverseNodeHandle : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour nodeBehaviour;
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private SphereCollider handleCollider;
        [SerializeField] private float minWorldRadius = 0.022f;
        [SerializeField] private float releaseGraceSecs = 0.2f;

        private int interactorId = -1;
        private float selectedUntil;

        public IRouteWorldNode Node => (IRouteWorldNode)nodeBehaviour;
        public bool IsSelected => selectedUntil > Time.unscaledTime;
        public int InteractorId => interactorId;
        public Pose GrabPose => new(grabbable.transform.position, grabbable.transform.rotation);

        private void OnEnable()
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }

        private void OnDisable()
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
            selectedUntil = 0f;
            interactorId = -1;
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (pointer.Type is PointerEventType.Select or PointerEventType.Unselect or PointerEventType.Cancel)
                Debug.Log($"[UniverseNodeHandle] {Node.Id} {pointer.Type} id={pointer.Identifier}");

            switch (pointer.Type)
            {
                case PointerEventType.Select:
                    selectedUntil = float.PositiveInfinity;
                    interactorId = pointer.Identifier;
                    break;
                case PointerEventType.Unselect:
                case PointerEventType.Cancel:
                    if (pointer.Identifier == interactorId)
                        selectedUntil = Time.unscaledTime + releaseGraceSecs;
                    break;
            }
        }

        private void LateUpdate()
        {
            if (!IsSelected && interactorId != -1)
            {
                interactorId = -1;
                ResetProxy();
            }

            if (!Node.IsActive)
                return;

            float worldScale = transform.lossyScale.x;
            if (worldScale <= 0.0001f)
                return;

            float localNodeRadius = Node.NodeBody.Radius / worldScale;
            float localMinRadius = minWorldRadius / worldScale;
            handleCollider.radius = Mathf.Max(localNodeRadius, localMinRadius);
        }

        private void ResetProxy()
        {
            grabbable.transform.localPosition = Vector3.zero;
            grabbable.transform.localRotation = Quaternion.identity;
        }
    }
}
