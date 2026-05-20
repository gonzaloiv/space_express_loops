using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public static class RoutePathUtility
    {
        public static Vector3[] CloseToLoopStart(Vector3[] segment, Vector3 loopStart)
        {
            if (segment == null || segment.Length == 0)
                return segment;

            if ((segment[^1] - loopStart).sqrMagnitude < 0.0001f)
                return segment;

            Vector3[] closed = new Vector3[segment.Length + 1];
            segment.CopyTo(closed, 0);
            closed[^1] = loopStart;
            return closed;
        }
    }
}
