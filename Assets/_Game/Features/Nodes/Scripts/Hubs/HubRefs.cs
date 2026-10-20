using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class HubRefs : MonoBehaviour
    {
        [SerializeField] private Transform spawnAnchor;
        [SerializeField] private NodeBody nodeBody;

        public Transform SpawnAnchor => spawnAnchor;
        public NodeBody NodeBody => nodeBody;
    }
}
