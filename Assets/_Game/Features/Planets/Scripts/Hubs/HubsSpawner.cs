using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class HubsSpawner : MonoBehaviour
    {
        [SerializeField] private MrukRoomLocalPlacement roomPlacement;
        [SerializeField] private List<HubBehaviour> hubs;
        [SerializeField] private float minDistanceBetweenHubs;
        [SerializeField] private MinMaxFloat localHeightRange;
        [SerializeField] private float minDistanceToRoomBounds;

        private IdCounter idCounter = new();

        public IReadOnlyList<HubBehaviour> All => hubs;

        public void SyncIdsFromSnapshot(IEnumerable<string> existingIds) => idCounter.SyncFromExistingIds(existingIds);

        public HubBehaviour SpawnNew()
        {
            string id = idCounter.NextId;
            Vector3 localPosition = GetValidHubLocalPosition();
            return ActivateHub(id, localPosition);
        }

        public HubData CreateHubData(HubBehaviour hub)
        {
            return new HubData
            {
                id = hub.Id,
                localPosition = SerializableVector3.FromVector3(hub.LocalPosition)
            };
        }

        public HubBehaviour SpawnWithId(string hubId, HubData hubData = null)
        {
            HubBehaviour existing = GetById(hubId);
            if (existing != null)
            {
                ReactivateHub(existing, hubId, hubData);
                return existing;
            }

            return ActivateHub(hubId, ResolveLocalPosition(hubData));
        }

        public Vector3 ResolveLocalPosition(HubData hubData = null)
        {
            return hubData?.localPosition != null
                ? hubData.localPosition.ToVector3()
                : GetValidHubLocalPosition();
        }

        public HubBehaviour GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            return hubs.FirstOrDefault(h => string.Equals(h.Id, id, StringComparison.Ordinal));
        }

        public void HideAll()
        {
            foreach (HubBehaviour hub in hubs)
            {
                if (hub.IsActive)
                    roomPlacement.Unregister(hub.LocalPosition, minDistanceToRoomBounds);
                hub.ResetForPool();
            }
        }

        private void ReactivateHub(HubBehaviour hub, string hubId, HubData hubData)
        {
            Vector3 localPosition = ResolveLocalPosition(hubData);
            hub.SpawnAsHub(hubId, localPosition);
            roomPlacement.Register(localPosition, minDistanceToRoomBounds);
        }

        private HubBehaviour ActivateHub(string id, Vector3 localPosition)
        {
            HubBehaviour hub = GetOrInstantiate();
            ReactivateHub(hub, id, new HubData
            {
                id = id,
                localPosition = SerializableVector3.FromVector3(localPosition)
            });
            return hub;
        }

        private Vector3 GetValidHubLocalPosition()
        {
            for (int i = 0; i < MrukRoomLocalPlacement.DefaultMaxIterations; i++)
            {
                Vector3 localPosition = roomPlacement.GetValidLocalPosition(minDistanceToRoomBounds, localHeightRange);
                if (minDistanceBetweenHubs <= 0f || !IsTooCloseToOtherHubs(localPosition, minDistanceBetweenHubs))
                    return localPosition;
            }

            Debug.LogWarning("Failed to find a hub position with minimum hub spacing; using best-effort placement.");
            return roomPlacement.GetValidLocalPosition(minDistanceToRoomBounds, localHeightRange);
        }

        private bool IsTooCloseToOtherHubs(Vector3 localPosition, float minDistance)
        {
            foreach (HubBehaviour hub in hubs)
            {
                if (!hub.IsActive)
                    continue;

                if (Vector3.Distance(hub.LocalPosition, localPosition) < minDistance)
                    return true;
            }

            return false;
        }

        private HubBehaviour GetOrInstantiate()
        {
            HubBehaviour hub = hubs.FirstOrDefault(h => !h.IsActive);
            if (hub == null)
            {
                hub = Instantiate(hubs[0], transform);
                hub.ResetForPool();
                hubs.Add(hub);
            }

            return hub;
        }
    }
}
