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
            public bool isProximityTarget;
        }

        [SerializeField] private List<Occupant> occupants = new();
        [SerializeField] private float floorMargin = 0.5f;
        [SerializeField] private float ceilingMargin = 0.5f;
        [SerializeField] private float minDistanceRadiusMultiplier = 1.5f;

        private readonly RoomLocalYRangeCache yRangeCache = new();

        public Vector3 RoomCenterLocalPosition
        {
            get
            {
                MRUKRoom room = GetCurrentRoom();
                if (room == null)
                    return Vector3.zero;

                return transform.InverseTransformPoint(room.Center());
            }
        }

        public void Clear()
        {
            occupants.Clear();
            yRangeCache.Invalidate();
        }

        public void Register(Vector3 localPosition, float radius, bool isProximityTarget = true) =>
            occupants.Add(new Occupant
            {
                localPosition = localPosition,
                radius = radius,
                isProximityTarget = isProximityTarget
            });

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

        public Vector3 GetValidLocalPosition(
            float radius,
            float? maxDistanceFromOccupants = null,
            int maxIterations = DefaultMaxIterations) =>
            FindValidLocalPosition(radius, maxDistanceFromOccupants, maxIterations);

        public bool IsValidLocalPosition(
            Vector3 localPosition,
            float radius,
            float? maxDistanceFromOccupants = null)
        {
            MRUKRoom room = GetCurrentRoom();
            return room != null && IsValidCandidate(localPosition, radius, maxDistanceFromOccupants, room, true);
        }

        public bool TryGetHeightRange(float radius, float margin, out float lowest, out float highest)
        {
            MRUKRoom room = GetCurrentRoom();
            if (room == null || !yRangeCache.TryGet(room, transform, out float minY, out float maxY))
            {
                lowest = 0f;
                highest = 0f;
                return false;
            }

            lowest = minY + margin + radius;
            highest = maxY - margin - radius;
            return highest > lowest;
        }

        public Vector3 GetRoomCenterLocalPosition(float radius)
        {
            MRUKRoom room = GetCurrentRoom();
            if (room == null)
            {
                Debug.LogWarning("No MRUK room available for placement.");
                return Vector3.zero;
            }

            Vector3 localPosition = ClampToValidHeight(RoomCenterLocalPosition, radius, room);

            if (!OverlapsOccupant(localPosition, radius))
                return localPosition;

            Debug.LogWarning("Room center overlaps an occupant; using random placement.");
            return GetValidLocalPosition(radius);
        }

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
                if (IsValidCandidate(localPosition, radius, maxDistanceFromOccupants, room, false))
                    return localPosition;
            }

            Debug.LogWarning("Failed to find a valid local position; defaulting to local origin.");
            return Vector3.zero;
        }

        private static MRUKRoom GetCurrentRoom() =>
            MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;

        private bool IsValidCandidate(
            Vector3 localPosition,
            float radius,
            float? maxDistanceFromOccupants,
            MRUKRoom room,
            bool checkWallClearance)
        {
            if (checkWallClearance && !IsClearOfRoomWalls(localPosition, radius, room))
                return false;

            if (maxDistanceFromOccupants.HasValue && !IsNearAnOccupant(localPosition, maxDistanceFromOccupants.Value))
                return false;
            if (OverlapsOccupant(localPosition, radius))
                return false;
            if (!IsWithinHeightRange(localPosition, radius, room))
                return false;
            return true;
        }

        private bool IsClearOfRoomWalls(Vector3 localPosition, float radius, MRUKRoom room)
        {
            Vector3 worldPosition = transform.TransformPoint(localPosition);
            return room.IsPositionInRoom(worldPosition, true)
                && IsClearOfWalls(room, worldPosition, radius);
        }

        private static bool IsClearOfWalls(MRUKRoom room, Vector3 worldPosition, float radius)
        {
            float clearance = radius;
            return room.IsPositionInRoom(worldPosition + Vector3.right * clearance, true)
                && room.IsPositionInRoom(worldPosition - Vector3.right * clearance, true)
                && room.IsPositionInRoom(worldPosition + Vector3.forward * clearance, true)
                && room.IsPositionInRoom(worldPosition - Vector3.forward * clearance, true);
        }

        private bool IsNearAnOccupant(Vector3 localPosition, float maxDistance)
        {
            if (occupants.Count == 0)
                return true;

            float closest = float.MaxValue;
            foreach (Occupant occupant in occupants)
            {
                if (!occupant.isProximityTarget)
                    continue;

                closest = Mathf.Min(closest, Vector3.Distance(localPosition, occupant.localPosition));
            }

            return closest < maxDistance;
        }

        private bool OverlapsOccupant(Vector3 localPosition, float radius)
        {
            foreach (Occupant occupant in occupants)
            {
                float minSeparation = (radius + occupant.radius) * minDistanceRadiusMultiplier;
                if ((occupant.localPosition - localPosition).sqrMagnitude < minSeparation * minSeparation)
                    return true;
            }

            return false;
        }

        private bool IsWithinHeightRange(Vector3 localPosition, float radius, MRUKRoom room) =>
            TryGetHeightRange(room, radius, out float lowest, out float highest)
            && localPosition.y >= lowest
            && localPosition.y <= highest;

        private Vector3 ClampToValidHeight(Vector3 localPosition, float radius, MRUKRoom room)
        {
            if (!TryGetHeightRange(room, radius, out float lowest, out float highest))
                return localPosition;

            localPosition.y = Mathf.Clamp(localPosition.y, lowest, highest);
            return localPosition;
        }

        private bool TryGetHeightRange(MRUKRoom room, float radius, out float lowest, out float highest)
        {
            lowest = 0f;
            highest = 0f;
            if (!yRangeCache.TryGet(room, transform, out float minY, out float maxY))
                return false;

            lowest = minY + floorMargin + radius;
            highest = maxY - ceilingMargin - radius;
            return highest > lowest;
        }
    }

    public static class MRUKRoomExtensions
    {
        public static Vector3 Center(this MRUKRoom room) => room.GetRoomBounds().center;
    }
}
