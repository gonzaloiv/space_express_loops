using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    [RequireComponent(typeof(HubRefs))]
    public class HubBehaviour : MonoBehaviour, IRouteWorldNode, IPoolableNode
    {
        [SerializeField] private HubRefs refs;

        private string id;

        public NodeBody NodeBody => refs.NodeBody;
        public bool IsActive => gameObject.activeInHierarchy;
        public string Id => id;
        public Vector3 LocalPosition => transform.localPosition;
        public Vector3 Position => refs.NodeBody.Position;
        public Pose SpawnPose => refs.SpawnAnchor.ToWorldPose();

        public void Initialize() => ResetRouteColor();

        public void Spawn(string hubId, Vector3 localPosition)
        {
            id = hubId;
            transform.localPosition = localPosition;
            gameObject.SetActive(true);
            refs.NodeBody.EnsureReadyForRouteColor();
        }

        public void ApplyRouteColor(Color color) => refs.NodeBody.SetRouteColor(color);

        public void ResetRouteColor() => refs.NodeBody.ResetRouteColor();

        public void Hide()
        {
            id = string.Empty;
            ResetRouteColor();
            gameObject.SetActive(false);
        }
    }
}
