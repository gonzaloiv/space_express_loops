using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SilentPlanets
{
    public class SilentPlanetLayout
    {
        private readonly SeededRandom random;
        private readonly Vector3 anchorLocal;
        private readonly float lowestY;
        private readonly float highestY;
        private readonly float ringRadius;
        private readonly float ringJitter;

        public SilentPlanetLayout(
            SeededRandom random,
            Vector3 anchorLocal,
            float lowestY,
            float highestY,
            float ringRadius,
            float ringJitter)
        {
            this.random = random;
            this.anchorLocal = anchorLocal;
            this.lowestY = lowestY;
            this.highestY = highestY;
            this.ringRadius = ringRadius;
            this.ringJitter = ringJitter;
        }

        public Vector3[] BuildClusterCenters(int count, int clusterCount)
        {
            if (count <= 0)
                return System.Array.Empty<Vector3>();

            int centerCount = Mathf.Clamp(clusterCount, 1, Mathf.Max(1, count));
            Vector3[] centers = new Vector3[centerCount];
            for (int i = 0; i < centerCount; i++)
                centers[i] = BuildCenter(i, centerCount);

            return centers;
        }

        public float NextY() => random.Range(lowestY, highestY);

        private Vector3 BuildCenter(int index, int centerCount)
        {
            float angle = (Mathf.PI * 2f * index / centerCount) + random.Range(-0.35f, 0.35f);
            float distance = ringRadius + random.Range(0f, ringJitter);
            return new Vector3(
                anchorLocal.x + Mathf.Cos(angle) * distance,
                anchorLocal.y,
                anchorLocal.z + Mathf.Sin(angle) * distance);
        }
    }
}
