using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class MrukRoomLocalPlacement : MonoBehaviour
    {
        public const int DefaultMaxIterations = 111;

        [Serializable]
        public class Occupant
        {
            public Vector3 localPosition;
            public float radius;
        }

        [SerializeField] private List<Occupant> occupants;
        [SerializeField] private float floorMargin = 0.5f;
        [SerializeField] private float ceilingMargin = 0.5f;

        public void Clear() => occupants.Clear();

        public void Register(Vector3 localPosition, float radius)
        {
            occupants.Add(new Occupant { localPosition = localPosition, radius = radius });
        }

        public void Unregister(Vector3 localPosition, float radius)
        {
            for (int i = occupants.Count - 1; i >= 0; i--)
            {
                Occupant occupant = occupants[i];
                if (Mathf.Approximately(occupant.radius, radius)
                    && Vector3.Distance(occupant.localPosition, localPosition) < 0.001f)
                {
                    occupants.RemoveAt(i);
                    return;
                }
            }
        }

        public void SyncFromSnapshot(IEnumerable<PlanetData> planets, IEnumerable<HubData> hubs, float hubPlacementRadius)
        {
            Clear();
            if (planets != null)
            {
                foreach (PlanetData planet in planets)
                    Register(planet.localPosition.ToVector3(), planet.radius);
            }
            if (hubs != null)
            {
                foreach (HubData hub in hubs)
                    Register(hub.localPosition.ToVector3(), hubPlacementRadius);
            }
        }

        public Vector3 GetValidLocalPosition(float radius, int maxIterations = DefaultMaxIterations)
        {
            return FindValidLocalPosition(radius, maxDistanceBetweenPlanets: null, maxIterations);
        }

        public Vector3 GetValidLocalPosition(float radius, float maxDistanceBetweenPlanets, int maxIterations = DefaultMaxIterations)
        {
            return FindValidLocalPosition(radius, maxDistanceBetweenPlanets, maxIterations);
        }

        private Vector3 FindValidLocalPosition(float radius, float? maxDistanceBetweenPlanets, int maxIterations)
        {
            MRUKRoom room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;
            if (room == null)
            {
                Debug.LogWarning("No MRUK room available for placement.");
                return Vector3.zero;
            }

            Vector3 result = Vector3.zero;
            for (int i = 0; i < maxIterations && result == Vector3.zero; i++)
            {
                Vector3? candidate = room.GenerateRandomPositionInRoom(radius, true);
                if (!candidate.HasValue)
                    continue;

                Vector3 localPos = transform.InverseTransformPoint(candidate.Value);
                if (maxDistanceBetweenPlanets.HasValue && !IsWithinMaxDistanceOfOccupants(localPos, maxDistanceBetweenPlanets.Value))
                    continue;

                if (Overlaps(localPos, radius))
                    continue;

                if (!IsWithinLocalHeightRange(localPos, radius))
                    continue;

                result = localPos;
            }

            if (result == Vector3.zero)
                Debug.LogWarning("Failed to find a valid local position; defaulting to local origin.");
            return result;
        }

        private bool IsWithinLocalHeightRange(Vector3 localPos, float radius)
        {
            MRUKRoom room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;
            if (room == null)
                return true;

            Bounds bounds = room.GetRoomBounds();
            if (bounds.size.sqrMagnitude <= 0f)
                return true;

            GetLocalYRangeFromBounds(bounds, out float roomMinY, out float roomMaxY);
            float minY = roomMinY + floorMargin + radius;
            float maxY = roomMaxY - ceilingMargin - radius;
            if (maxY <= minY)
                return false;

            return localPos.y >= minY && localPos.y <= maxY;
        }

        private void GetLocalYRangeFromBounds(Bounds bounds, out float minLocalY, out float maxLocalY)
        {
            minLocalY = float.PositiveInfinity;
            maxLocalY = float.NegativeInfinity;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(extents, new Vector3(sx, sy, sz));
                        float localY = transform.InverseTransformPoint(corner).y;
                        minLocalY = Mathf.Min(minLocalY, localY);
                        maxLocalY = Mathf.Max(maxLocalY, localY);
                    }
        }

        private bool IsWithinMaxDistanceOfOccupants(Vector3 localPos, float maxDistance)
        {
            if (occupants.Count == 0)
                return true;

            float minDistance = float.MaxValue;
            foreach (Occupant occupant in occupants)
                minDistance = Mathf.Min(minDistance, Vector3.Distance(localPos, occupant.localPosition));

            return minDistance < maxDistance;
        }

        private bool Overlaps(Vector3 localPos, float radius)
        {
            foreach (Occupant occupant in occupants)
            {
                if (Vector3.Distance(occupant.localPosition, localPos) < radius + occupant.radius)
                    return true;
            }

            return false;
        }
    }
}
