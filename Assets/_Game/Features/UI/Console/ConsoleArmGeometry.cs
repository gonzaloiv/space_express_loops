using UnityEngine;

namespace DigitalLove.Game.UI.Console
{
    // The console's pair geometry: one corner on the grid's bottom-left-back, a 45° arm out to the
    // left and a vertical arm. Values are metres in the console's parent space; the arms are authored
    // line renderers and the windows hang from their ends.
    public static class ConsoleArmGeometry
    {
        public const float DiagonalDegrees = 45f;

        // The grid's bottom-left-back corner, from the field's frame (centre and half in the same space).
        public static Vector3 Corner(Vector3 fieldCentre, float half) =>
            fieldCentre + new Vector3(-half, -half, half);

        // The 45° arm's end, outward-left and up in the field's back plane.
        public static Vector3 DiagonalEnd(Vector3 corner, Quaternion rotation, float length) =>
            corner + rotation * (new Vector3(-1f, 1f, 0f).normalized * length);

        // The vertical arm's end, just outside the corner, rising straight up.
        public static Vector3 VerticalEnd(
            Vector3 corner, Quaternion rotation, float length, float gap) =>
            corner + rotation * new Vector3(-gap, length, 0f);

        // The bottom of the vertical arm: just outside the corner.
        public static Vector3 VerticalFoot(Vector3 corner, Quaternion rotation, float gap) =>
            corner + rotation * new Vector3(-gap, 0f, 0f);

        // Where a window's centre goes so its top-inner corner sits at the arm's end. The window's
        // local +x is the field's +x (the inner edge) and its local +y is up.
        public static Vector3 WindowCentre(Vector3 topInner, Quaternion rotation, Vector2 size) =>
            topInner + rotation * new Vector3(-size.x * 0.5f, -size.y * 0.5f, 0f);
    }
}
