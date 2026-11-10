using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One arm of the field meter on the grid's base edge, relative to the grid's centre. An arm runs
    // from the front-centre (s = 0, the player's side, -z) around one side to the back-centre (s = 1);
    // side is -1 for the left arm and +1 for the right. Equal steps in s are equal lengths.
    public static class FieldEdgePath
    {
        public static Vector3 Point(float s, float side, float half)
        {
            float distance = Mathf.Clamp01(s) * 4f * half;
            if (distance <= half)
                return new Vector3(side * distance, -half, -half);

            if (distance <= 3f * half)
                return new Vector3(side * half, -half, distance - 2f * half);

            return new Vector3(side * (4f * half - distance), -half, half);
        }

        // Where tick i sits on an arm split into (ticks + 1) equal bands.
        public static float TickShare(int index, int ticks) =>
            (index + 1f) / (ticks + 1f);
    }
}
