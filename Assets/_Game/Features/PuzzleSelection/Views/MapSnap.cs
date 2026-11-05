using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal static class MapSnap
    {
        public static int Nearest(PuzzleMap map, Transform mapRoot, Vector3 point)
        {
            int nearest = -1;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < map.Count; i++)
            {
                float distance = Vector3.Distance(
                    mapRoot.TransformPoint(map.Position(i)), point);
                if (distance >= nearestDistance)
                    continue;

                nearestDistance = distance;
                nearest = i;
            }

            return nearest;
        }
    }
}
