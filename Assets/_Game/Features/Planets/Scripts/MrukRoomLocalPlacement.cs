using System.Collections.Generic;
using DigitalLove.Global;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class MrukRoomLocalPlacement : MonoBehaviour
    {
        public const int DefaultMaxIterations = 333;

        private struct Occupant
        {
            public Vector3 localPosition;
            public float radius;
        }

        private readonly List<Occupant> occupants = new();

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
            return FindValidLocalPosition(radius, maxDistanceFromRootWorld: null, localHeightRange: null, maxIterations);
        }

        public Vector3 GetValidLocalPosition(float radius, float maxDistanceFromRootWorld, int maxIterations = DefaultMaxIterations)
        {
            return FindValidLocalPosition(radius, maxDistanceFromRootWorld, localHeightRange: null, maxIterations);
        }

        public Vector3 GetValidLocalPosition(float radius, MinMaxFloat localHeightRange, int maxIterations = DefaultMaxIterations)
        {
            return FindValidLocalPosition(radius, maxDistanceFromRootWorld: null, localHeightRange, maxIterations);
        }

        private Vector3 FindValidLocalPosition(float radius, float? maxDistanceFromRootWorld, MinMaxFloat localHeightRange, int maxIterations)
        {
            Vector3 result = Vector3.zero;
            for (int i = 0; i < maxIterations && result == Vector3.zero; i++)
            {
                Vector3? candidate = MRUK.Instance.GetCurrentRoom().GenerateRandomPositionInRoom(radius, true);
                if (!candidate.HasValue)
                    continue;

                float distance = Vector3.Distance(candidate.Value, transform.position);
                if (distance <= radius * 3)
                    continue;

                if (maxDistanceFromRootWorld.HasValue && distance >= maxDistanceFromRootWorld.Value)
                    continue;

                Vector3 localPos = transform.InverseTransformPoint(candidate.Value);
                if (Overlaps(localPos, radius))
                    continue;

                if (!IsWithinLocalHeightRange(localPos, localHeightRange))
                    continue;

                result = localPos;
            }

            if (result == Vector3.zero)
                Debug.LogWarning("Failed to find a valid local position; defaulting to local origin.");
            return result;
        }

        private static bool IsWithinLocalHeightRange(Vector3 localPos, MinMaxFloat localHeightRange)
        {
            if (localHeightRange == null || localHeightRange.max <= localHeightRange.min)
                return true;

            return localPos.y >= localHeightRange.min && localPos.y <= localHeightRange.max;
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
