using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    internal static class ZoneGesture
    {
        public static bool HandInsideContent(SpaceZoneView zone, Grabbable grabbable) =>
            zone.ContentContains(HandWorld(zone, grabbable));

        public static bool AllHandsInside(SpaceZoneView zone, Grabbable grabbable)
        {
            for (int i = 0; i < grabbable.GrabPoints.Count; i++)
            {
                if (!zone.ContentContains(grabbable.GrabPoints[i].position))
                    return false;
            }

            return true;
        }

        public static Vector3 HandWorld(SpaceZoneView zone, Grabbable grabbable) =>
            grabbable.GrabPoints.Count > 0 ? grabbable.GrabPoints[0].position : zone.transform.position;

        public static Vector3 HandLocal(SpaceZoneView zone, Grabbable grabbable) =>
            zone.ToLocal(HandWorld(zone, grabbable));

        public static float HandDistance(Grabbable grabbable)
        {
            if (grabbable.GrabPoints.Count < 2)
                return 0f;

            return Vector3.Distance(grabbable.GrabPoints[0].position, grabbable.GrabPoints[1].position);
        }

        public static float ScaleTarget(float startScale, float ratio, float gain, float min, float max) =>
            Mathf.Clamp(startScale * Mathf.Pow(ratio, gain), min, max);

        public static float ResizeFactor(float ratio, float gain, float maxGrow) =>
            Mathf.Clamp(Mathf.Pow(ratio, gain), 1f, maxGrow);
    }
}
