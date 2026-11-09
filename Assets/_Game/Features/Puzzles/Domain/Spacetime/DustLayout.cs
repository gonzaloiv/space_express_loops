using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The field dust's rest data, drawn once from a fixed seed: the same dust on every relaunch.
    // Rest positions are fractions of the dust's reach in [-1, 1] per axis; drift is a pure function
    // of time, so nothing is simulated or stored.
    public class DustLayout
    {
        private const float TwoPi = Mathf.PI * 2f;

        private readonly Vector3[] fractions;
        private readonly Vector3[] phases;
        private readonly float[] weights;

        public DustLayout(int count, int seed)
        {
            fractions = new Vector3[count];
            phases = new Vector3[count];
            weights = new float[count];
            System.Random random = new(seed);
            for (int i = 0; i < count; i++)
            {
                fractions[i] = new Vector3(Signed(random), Signed(random), Signed(random));
                phases[i] = new Vector3(Unit(random), Unit(random), Unit(random)) * TwoPi;
                weights[i] = Unit(random);
            }
        }

        public int Count => fractions.Length;

        public Vector3 Rest(int index) => fractions[index];

        public float Weight(int index) => weights[index];

        public Vector3 Drift(int index, float time, float frequency)
        {
            Vector3 phase = phases[index];
            float t = time * frequency * TwoPi;
            return new Vector3(
                Mathf.Sin(t + phase.x),
                Mathf.Sin(t * 0.83f + phase.y),
                Mathf.Sin(t * 0.71f + phase.z));
        }

        // 1 inside the fade start, easing to 0 at the edge of the reach, so the box never shows.
        public float EdgeFade(int index, float fadeStart)
        {
            Vector3 f = fractions[index];
            float edge = Mathf.Max(Mathf.Abs(f.x), Mathf.Max(Mathf.Abs(f.y), Mathf.Abs(f.z)));
            if (fadeStart >= 1f)
                return 1f;

            return 1f - Mathf.SmoothStep(0f, 1f, (edge - fadeStart) / (1f - fadeStart));
        }

        private static float Signed(System.Random random) => Unit(random) * 2f - 1f;

        private static float Unit(System.Random random) => (float)random.NextDouble();
    }
}
