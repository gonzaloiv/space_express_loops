using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class PuzzleViewFrame
    {
        public static Quaternion Rotation(Vector3 origin, Vector3 destination)
        {
            Vector3 run = destination - origin;
            Vector3 runDirection = run.sqrMagnitude > 0.000001f ? run.normalized : Vector3.right;
            Vector3 localUp = Vector3.up - runDirection * Vector3.Dot(Vector3.up, runDirection);
            localUp = localUp.sqrMagnitude > 0.000001f ? localUp.normalized : Vector3.forward;

            Vector3 right = Vector3.Cross(runDirection, localUp);
            return Quaternion.LookRotation(right, localUp);
        }
    }
}
