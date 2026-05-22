using System;
using System.Collections.Generic;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    [RequireComponent(typeof(HubRefs))]
    public class HubBehaviour : MonoBehaviour, IRouteWorldNode
    {
        [SerializeField] private HubRefs refs;

        private string id;

        public NodeBody NodeBody => refs.nodeBody;
        public bool IsActive => gameObject.activeInHierarchy;
        public string Id => id;
        public Vector3 LocalPosition => transform.localPosition;
        public Vector3 Position => refs.nodeBody != null ? refs.nodeBody.Position : transform.position;
        public Pose SpawnPose => refs.spawnAnchor.ToWorldPose();

        private void Awake()
        {
            if (refs == null)
                refs = GetComponent<HubRefs>();
        }

        public void Initialize() => ResetRouteColor();

        public void Spawn(string hubId, Vector3 localPosition)
        {
            id = hubId;
            transform.localPosition = localPosition;
            gameObject.SetActive(true);
            refs.nodeBody?.EnsureReadyForRouteColor();
        }

        public void ApplyRouteColor(Color color) => refs.nodeBody?.SetRouteColor(color);

        public void ResetRouteColor() => refs.nodeBody?.ResetRouteColor();

        public void Hide()
        {
            id = string.Empty;
            ResetRouteColor();
            gameObject.SetActive(false);
        }
    }

    public static class HubBehaviourPoolExtensions
    {
        public static void ForEachInPool(this IList<HubBehaviour> pool, Action<HubBehaviour> action)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null)
                    action(pool[i]);
            }
        }

        public static HubBehaviour AcquireInactive(
            this List<HubBehaviour> pool,
            HubBehaviour prototype,
            Transform parent,
            Action<HubBehaviour> onInstantiated)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && !pool[i].IsActive)
                    return pool[i];
            }

            HubBehaviour created = UnityEngine.Object.Instantiate(prototype, parent);
            onInstantiated(created);
            pool.Add(created);
            return created;
        }
    }
}
