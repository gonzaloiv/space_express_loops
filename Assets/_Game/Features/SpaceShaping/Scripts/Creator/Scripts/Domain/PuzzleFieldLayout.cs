using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleFieldLayout
    {
        public static float RunLength(PuzzleFieldPlanet[] planets, float distance, float captureRadius)
        {
            float half = Mathf.Max(distance * 0.5f, Clearance(planets, captureRadius));
            return half * 2f;
        }

        private static float Clearance(PuzzleFieldPlanet[] planets, float captureRadius)
        {
            float clearance = 0f;
            for (int i = 0; i < planets.Length; i++)
            {
                float reach = planets[i].position.magnitude + planets[i].radius + captureRadius;
                clearance = Mathf.Max(clearance, reach);
            }

            return clearance;
        }
    }
}
