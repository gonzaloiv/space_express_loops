using System;
using System.Collections.Generic;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class HubsSpawner : MonoBehaviour
    {
        [SerializeField] private List<HubBehaviour> hubs;

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

        public HubBehaviour GetFirstActive()
        {
            for (int i = 0; i < hubs.Count; i++)
            {
                HubBehaviour hub = hubs[i];
                if (hub != null && hub.IsActive)
                    return hub;
            }

            return null;
        }

        public HubBehaviour GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            for (int i = 0; i < hubs.Count; i++)
            {
                HubBehaviour hub = hubs[i];
                if (hub != null && string.Equals(hub.Id, id, StringComparison.Ordinal))
                    return hub;
            }

            return null;
        }

        public void SpawnHubs(IEnumerable<HubData> hubDataList)
        {
            if (hubDataList == null)
                return;

            foreach (HubData hubData in hubDataList)
                GetOrSpawn(hubData.id, hubData);
        }

        public void HideAll() =>
            hubs.ForEachInPool(hub => hub.Hide());

        private HubBehaviour Spawn(string hubId, HubData hubData = null)
        {
            HubBehaviour hub = hubs.AcquireInactive(hubs[0], transform, CreatePooledHub);
            Vector3 localPosition = hubData?.localPosition != null
                ? hubData.localPosition.ToVector3()
                : Vector3.zero;
            hub.Spawn(hubId, localPosition);
            return hub;
        }

        private static void CreatePooledHub(HubBehaviour hub)
        {
            hub.Initialize();
            hub.Hide();
        }
    }
}
