using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public static class CubeGesture
    {
        public static bool HandInsideScale(CubeView zone, Grabbable grabbable) =>
            zone.ScaleContains(HandWorld(zone, grabbable));

        // Two hands straddle a small cube, so the test is the point between the pinches, not each hand.
        public static bool PinchCentreInsideScale(CubeView zone, Grabbable grabbable) =>
            zone.ScaleContains(PinchCentre(zone, grabbable));

        private static Vector3 PinchCentre(CubeView zone, Grabbable grabbable)
        {
            if (grabbable.GrabPoints.Count < 2)
                return HandWorld(zone, grabbable);

            return (grabbable.GrabPoints[0].position + grabbable.GrabPoints[1].position) * 0.5f;
        }

        public static Vector3 HandWorld(CubeView zone, Grabbable grabbable) =>
            grabbable.GrabPoints.Count > 0 ? grabbable.GrabPoints[0].position : zone.transform.position;

        public static Vector3 HandLocal(CubeView zone, Grabbable grabbable) =>
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
