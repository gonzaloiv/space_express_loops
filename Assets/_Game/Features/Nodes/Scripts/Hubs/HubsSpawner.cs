using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class HubsSpawner : MonoBehaviour
    {
        [SerializeField] private MrukRoomLocalPlacement roomPlacement;
        [SerializeField] private List<HubBehaviour> hubs;
        [SerializeField] private float minDistanceBetweenHubs;
        [SerializeField] private float minDistanceToRoomBounds;

        private IdCounter idCounter = new();

        public IReadOnlyList<HubBehaviour> All => hubs;

        public void SyncIdsFromSnapshot(IEnumerable<string> existingIds) =>
            idCounter.SyncFromExistingIds(existingIds);

        public void ResetPool()
        {
            InitializePool();
            HideAll();
        }

        public void InitializePool() =>
            hubs.ForEachInPool(hub => hub.Initialize());

        public HubBehaviour SpawnNew() => Spawn(idCounter.NextId);

        public HubData CreateHubData(HubBehaviour hub) => new()
        {
            id = hub.Id,
            localPosition = SerializableVector3.FromVector3(hub.LocalPosition)
        };

        public HubBehaviour GetOrSpawn(string hubId, HubData hubData = null)
        {
            HubBehaviour existing = GetById(hubId);
            if (existing != null && existing.IsActive)
                return existing;

            return Spawn(hubId, hubData);
        }

        public HubBehaviour GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            return hubs.FirstOrDefault(h => string.Equals(h.Id, id, StringComparison.Ordinal));
        }

        public void HideAll()
        {
            hubs.ForEachInPool(hub =>
            {
                if (hub.IsActive)
                    roomPlacement.Unregister(hub.LocalPosition, minDistanceToRoomBounds);
                hub.Hide();
            });
        }

        private HubBehaviour Spawn(string hubId, HubData hubData = null)
        {
            HubBehaviour hub = hubs.AcquireInactive(hubs[0], transform, h =>
            {
                h.Initialize();
                h.Hide();
            });
            Vector3 localPosition = hubData?.localPosition != null
                ? hubData.localPosition.ToVector3()
                : GetValidHubLocalPosition();
            hub.Spawn(hubId, localPosition);
            roomPlacement.Register(localPosition, minDistanceToRoomBounds);
            return hub;
        }

        private Vector3 GetValidHubLocalPosition()
        {
            for (int i = 0; i < MrukRoomLocalPlacement.DefaultMaxIterations; i++)
            {
                Vector3 localPosition = roomPlacement.GetValidLocalPosition(minDistanceToRoomBounds);
                if (minDistanceBetweenHubs <= 0f || !IsTooCloseToOtherHubs(localPosition, minDistanceBetweenHubs))
                    return localPosition;
            }

            Debug.LogWarning("Failed to find a hub position with minimum hub spacing; using best-effort placement.");
            return roomPlacement.GetValidLocalPosition(minDistanceToRoomBounds);
        }

        private bool IsTooCloseToOtherHubs(Vector3 localPosition, float minDistance)
        {
            for (int i = 0; i < hubs.Count; i++)
            {
                HubBehaviour hub = hubs[i];
                if (hub == null || !hub.IsActive)
                    continue;

                if (Vector3.Distance(hub.LocalPosition, localPosition) < minDistance)
                    return true;
            }

            return false;
        }
    }
}
