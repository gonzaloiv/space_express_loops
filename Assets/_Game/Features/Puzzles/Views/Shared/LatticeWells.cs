using System;
using System.Collections.Generic;
using DigitalLove.Game.Spatial;
using Unity.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Tuning for the field-wells spike (closed 2026-11-11; keep or drop is a board open tail):
    // whether the lattice shows the planets' wells, and how deep.
    [Serializable]
    public class LatticeWells
    {
        // Below this the zones are at rest: the field is the job's start field.
        private const float RestOffset = 0.0001f;

        [Tooltip("Draw the field's wells on the lattice. Off is the zone-only lattice.")]
        public bool enabled;

        [Tooltip("Draw only the change from the job's start: the start lattice is flat, and a well " +
                 "appears where a planet grows and flattens where one shrinks. Off draws every well.")]
        public bool changeOnly = true;

        [Tooltip("Well depth: a node moves gain × √strength / softened distance toward each source.")]
        [Min(0f)] public float gain = 0.07f;

        [Tooltip("The most a node moves toward a source, as a fraction of its gap to the source's surface.")]
        [Range(0f, WellPull.MaxFractionCap)] public float maxFraction = 0.6f;

        [Tooltip("How much of the sun's well to draw. Its pull bends the whole grid; 0 draws the planets' " +
                 "wells only (nodes still stay out of the sun).")]
        [Range(0f, 1f)] public float sunWeight = 1f;

        // Fills the drawn sources (the sun weighted) and, at rest or on a fresh baseline, the baseline.
        public void Fill(
            SpaceShapingSession session, NativeArray<SpaceSource> sources,
            NativeArray<SpaceSource> baseline, bool freshBaseline)
        {
            if (!enabled)
                return;

            IReadOnlyList<SpaceSource> field = session.BuildField().Sources;
            for (int i = 0; i < sources.Length; i++)
            {
                SpaceSource source = field[i];
                if (session.IsSun(i))
                    source.Strength *= sunWeight;
                sources[i] = source;
            }

            if (freshBaseline || AtRest(session))
                sources.CopyTo(baseline);
        }

        // The wells to draw: none when off, every well, or only the change from the baseline.
        public WellPull Pull(NativeArray<SpaceSource> sources, NativeArray<SpaceSource> baseline)
        {
            NativeArray<SpaceSource> none = sources.GetSubArray(0, 0);
            NativeArray<SpaceSource> drawn = enabled ? sources : none;
            NativeArray<SpaceSource> start = enabled && changeOnly ? baseline : none;
            return new WellPull(drawn.AsReadOnly(), start.AsReadOnly(), gain, maxFraction);
        }

        private static bool AtRest(SpaceShapingSession session) =>
            Mathf.Abs(session.Zones.StrongestScaleOffset()) < RestOffset;
    }
}
