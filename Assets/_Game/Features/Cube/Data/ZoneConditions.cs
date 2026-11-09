using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public readonly struct ZoneConditions
    {
        public bool Translatable { get; }

        public bool Resizable { get; }

        public Vector3 Origin { get; }

        public Vector3 Destination { get; }

        // Moves stay on the cube's current height (the map's X-Z plane).
        public bool Planar { get; }

        // A puzzle cube must not swallow the run's endpoints; a map cube has none to keep out.
        public bool IgnoresEndpoints { get; }

        public ZoneConditions(bool translatable, bool resizable)
            : this(translatable, resizable, Vector3.zero, Vector3.zero)
        {
        }

        public ZoneConditions(bool translatable, bool resizable, Vector3 origin, Vector3 destination)
            : this(translatable, resizable, origin, destination, false, false)
        {
        }

        private ZoneConditions(
            bool translatable, bool resizable, Vector3 origin, Vector3 destination,
            bool planar, bool ignoresEndpoints)
        {
            Translatable = translatable;
            Resizable = resizable;
            Origin = origin;
            Destination = destination;
            Planar = planar;
            IgnoresEndpoints = ignoresEndpoints;
        }

        // The selection map's cube: slides on the map plane, never resizes, no endpoints to keep out.
        public static ZoneConditions SlideOnPlane() =>
            new(true, false, Vector3.zero, Vector3.zero, true, true);
    }
}
