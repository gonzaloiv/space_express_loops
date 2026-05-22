using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class MrukRoomLocalPlacement : MonoBehaviour
    {
        public const int DefaultMaxIterations = 111;

        [Serializable]
        private struct Occupant
        {
            public Vector3 localPosition;
            public float radius;
        }

        [SerializeField] private List<Occupant> occupants = new();
        [SerializeField] private float floorMargin = 0.5f;
        [SerializeField] private float ceilingMargin = 0.5f;
        [SerializeField] private float minDistanceRadiusMultiplier = 1.5f;

        private readonly RoomLocalYRangeCache yRangeCache = new();

        public void Clear()
        {
            occupants.Clear();
            yRangeCache.Invalidate();
        }

        public void Register(Vector3 localPosition, float radius) =>
            occupants.Add(new Occupant { localPosition = localPosition, radius = radius });

        public void Unregister(Vector3 localPosition, float radius)
        {
            int index = occupants.FindIndex(o =>
                Mathf.Approximately(o.radius, radius)
                && (o.localPosition - localPosition).sqrMagnitude < 0.000001f);
            if (index >= 0)
                occupants.RemoveAt(index);
        }

        public void SyncFromSnapshot(IEnumerable<HubData> hubs, IEnumerable<PlanetData> planets, float hubPlacementRadius)
        {
            Clear();
            if (hubs != null)
            {
                foreach (HubData hub in hubs)
                    Register(hub.localPosition.ToVector3(), hubPlacementRadius);
            }

            if (planets != null)
            {
                foreach (PlanetData planet in planets)
                    Register(planet.localPosition.ToVector3(), planet.radius);
            }
        }

        public Vector3 GetValidLocalPosition(float radius, int maxIterations = DefaultMaxIterations) =>
            FindValidLocalPosition(radius, maxDistanceFromOccupants: null, maxIterations);

        public Vector3 GetValidLocalPosition(float radius, float maxDistanceFromOccupants, int maxIterations = DefaultMaxIterations) =>
            FindValidLocalPosition(radius, maxDistanceFromOccupants, maxIterations);

        private Vector3 FindValidLocalPosition(float radius, float? maxDistanceFromOccupants, int maxIterations)
        {
            MRUKRoom room = GetCurrentRoom();
            if (room == null)
            {
                Debug.LogWarning("No MRUK room available for placement.");
                return Vector3.zero;
            }

            for (int i = 0; i < maxIterations; i++)
            {
                Vector3? worldPosition = room.GenerateRandomPositionInRoom(radius, true);
                if (!worldPosition.HasValue)
                    continue;

                Vector3 localPosition = transform.InverseTransformPoint(worldPosition.Value);
                if (IsValidCandidate(localPosition, radius, maxDistanceFromOccupants, room))
                    return localPosition;
            }

            Debug.LogWarning("Failed to find a valid local position; defaulting to local origin.");
            return Vector3.zero;
        }

        private static MRUKRoom GetCurrentRoom() =>
            MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;

        private bool IsValidCandidate(Vector3 localPosition, float radius, float? maxDistanceFromOccupants, MRUKRoom room)
        {
            if (maxDistanceFromOccupants.HasValue && !IsNearAnOccupant(localPosition, maxDistanceFromOccupants.Value))
                return false;
            if (OverlapsOccupant(localPosition, radius))
                return false;
            if (!IsWithinHeightRange(localPosition, radius, room))
                return false;
            return true;
        }

        private bool IsNearAnOccupant(Vector3 localPosition, float maxDistance)
        {
            if (occupants.Count == 0)
                return true;

            Debug.LogWarning($"maxDistance: {maxDistance}");
            float closest = float.MaxValue;
            foreach (Occupant occupant in occupants)
                closest = Mathf.Min(closest, Vector3.Distance(localPosition, occupant.localPosition));

            return closest < maxDistance;
        }

        private bool OverlapsOccupant(Vector3 localPosition, float radius)
        {
            foreach (Occupant occupant in occupants)
            {
                float minSeparation = (radius + occupant.radius) * minDistanceRadiusMultiplier;
                Debug.LogWarning($"minSeparation: {minSeparation}");
                if ((occupant.localPosition - localPosition).sqrMagnitude < minSeparation * minSeparation)
                    return true;
            }

            return false;
        }

        private bool IsWithinHeightRange(Vector3 localPosition, float radius, MRUKRoom room)
        {
            if (!yRangeCache.TryGet(room, transform, out float minY, out float maxY))
                return true;

            float lowest = minY + floorMargin + radius;
            float highest = maxY - ceilingMargin - radius;
            return highest > lowest && localPosition.y >= lowest && localPosition.y <= highest;
        }
    }
}
