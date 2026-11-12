using System;
using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
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

        [NonSerialized] private SpaceSource[] start;

        public WellPull? For(SpaceShapingSession session)
        {
            if (!enabled)
                return null;

            SpaceSource[] sources = Sources(session);
            if (!changeOnly)
                return new WellPull(sources, gain, maxFraction);

            if (start == null || start.Length != sources.Length || AtRest(session))
                start = sources;
            return new WellPull(sources, gain, maxFraction, start);
        }

        private static bool AtRest(SpaceShapingSession session) =>
            Mathf.Abs(session.Zones.StrongestScaleOffset()) < RestOffset;

        private SpaceSource[] Sources(SpaceShapingSession session)
        {
            IReadOnlyList<SpaceSource> field = session.BuildField().Sources;
            SpaceSource[] weighted = new SpaceSource[field.Count];
            for (int i = 0; i < weighted.Length; i++)
            {
                weighted[i] = field[i];
                if (session.IsSun(i))
                    weighted[i].Strength *= sunWeight;
            }

            return weighted;
        }
    }
}
