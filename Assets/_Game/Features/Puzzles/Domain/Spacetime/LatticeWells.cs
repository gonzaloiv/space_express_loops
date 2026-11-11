using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Tuning for the field-wells spike (docs/plans/in-progress/field-wells.md): whether the lattice
    // shows the planets' wells, and how deep.
    [Serializable]
    public class LatticeWells
    {
        [Tooltip("Draw the field's wells on the lattice. Off is the zone-only lattice.")]
        public bool enabled;

        [Tooltip("Well depth: a node moves gain × √strength / softened distance toward each source.")]
        [Min(0f)] public float gain = 0.07f;

        [Tooltip("The most a node moves toward a source, as a fraction of its gap to the source's surface.")]
        [Range(0f, 1f)] public float maxFraction = 0.6f;

        public WellPull? For(SpaceShapingSession session) =>
            enabled ? new WellPull(session.BuildField().Sources, gain, maxFraction) : null;
    }
}
