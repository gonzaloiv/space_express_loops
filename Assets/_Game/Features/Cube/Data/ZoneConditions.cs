using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public readonly struct ZoneConditions
    {
        public bool Translatable { get; }

        public bool Resizable { get; }

        // Every ship's endpoints; the cube may not influence any of them.
        public IReadOnlyList<Vector3> Endpoints { get; }

        // Moves stay on the cube's current height (the map's X-Z plane).
        public bool Planar { get; }

        // A puzzle cube must not swallow the run's endpoints; a map cube has none to keep out.
        public bool IgnoresEndpoints { get; }

        public ZoneConditions(bool translatable, bool resizable)
            : this(translatable, resizable, null, false, false)
        {
        }

        public ZoneConditions(
            bool translatable, bool resizable, IReadOnlyList<Vector3> endpoints)
            : this(translatable, resizable, endpoints, false, false)
        {
        }

        private ZoneConditions(
            bool translatable, bool resizable, IReadOnlyList<Vector3> endpoints,
            bool planar, bool ignoresEndpoints)
        {
            Translatable = translatable;
            Resizable = resizable;
            Endpoints = endpoints;
            Planar = planar;
            IgnoresEndpoints = ignoresEndpoints;
        }

        // The selection map's cube: slides on the map plane, never resizes, no endpoints to keep out.
        public static ZoneConditions SlideOnPlane() =>
            new(true, false, null, true, true);
    }
}
