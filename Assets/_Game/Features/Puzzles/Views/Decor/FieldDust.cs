using System;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Weightless dust in and around the field (Field Elements tier 3). The authored particle system
    // only draws: every frame each mote sits at its seeded rest position plus a slow drift, pushed
    // through the same warp as the lattice, so the cube shapes the dust and it adds no well.
    // The system must simulate in Local space and sit at identity under the field root.
    public class FieldDust : MonoBehaviour
    {
        private const float Lifetime = 1000f;

        [SerializeField] private ParticleSystem particles;
        [SerializeField, Min(1)] private int count = 400;
        [SerializeField] private int seed = 7;
        [Tooltip("How far the dust reaches, as a multiple of the field half-extent (above 1 spills around the field).")]
        [SerializeField, Min(0.1f)] private float spread = 1.3f;
        [Tooltip("Fraction of the reach where the dust starts fading out, so the box edge never shows.")]
        [SerializeField, Range(0f, 1f)] private float edgeFade = 0.6f;
        [Tooltip("Mote size in metres (local to the field root).")]
        [SerializeField, Min(0f)] private float size = 0.002f;
        [SerializeField, Range(0f, 1f)] private float sizeJitter = 0.5f;
        [SerializeField] private Color color = new(0.8f, 0.85f, 1f, 0.35f);
        [Tooltip("Drift amplitude as a fraction of the field half-extent.")]
        [SerializeField, Min(0f)] private float drift = 0.02f;
        [Tooltip("Drift cycles per second.")]
        [SerializeField, Min(0f)] private float driftFrequency = 0.05f;

        private DustLayout layout;
        private ParticleSystem.Particle[] buffer;
        private Func<Vector3, Vector3> warp;
        private Vector3 centre;
        private float half;

        private DustLayout Layout => layout ??= new DustLayout(count, seed);

        private ParticleSystem.Particle[] Buffer => buffer ??= new ParticleSystem.Particle[count];

        public void Render(Func<Vector3, Vector3> fieldWarp, Vector3 fieldCentre, float fieldHalf)
        {
            warp = fieldWarp;
            centre = fieldCentre;
            half = fieldHalf;
        }

        private void LateUpdate()
        {
            if (warp == null)
                return;

            float time = Time.unscaledTime;
            for (int i = 0; i < Layout.Count; i++)
                Buffer[i] = Place(i, time);

            particles.SetParticles(Buffer, Layout.Count);
        }

        private ParticleSystem.Particle Place(int index, float time)
        {
            Vector3 rest = centre
                + Layout.Rest(index) * (half * spread)
                + Layout.Drift(index, time, driftFrequency) * (half * drift);
            return new ParticleSystem.Particle
            {
                position = warp(rest),
                startSize = size * Mathf.Lerp(1f - sizeJitter, 1f, Layout.Weight(index)),
                startColor = color.WithAlpha(color.a * Layout.EdgeFade(index, edgeFade)),
                startLifetime = Lifetime,
                remainingLifetime = Lifetime,
            };
        }
    }
}
