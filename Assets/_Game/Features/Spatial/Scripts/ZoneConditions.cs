using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.Spatial
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

        // Where a move may put the cube's centre: the field's box, so a stray grab cannot carry the
        // cube off the field (onto the player's hands). No area, no limit.
        public bool HasMoveArea { get; }

        public Vector3 MoveAreaCentre { get; }

        public float MoveAreaHalf { get; }

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
            HasMoveArea = false;
            MoveAreaCentre = Vector3.zero;
            MoveAreaHalf = 0f;
        }

        private ZoneConditions(ZoneConditions source, Vector3 centre, float half)
        {
            Translatable = source.Translatable;
            Resizable = source.Resizable;
            Endpoints = source.Endpoints;
            Planar = source.Planar;
            IgnoresEndpoints = source.IgnoresEndpoints;
            HasMoveArea = true;
            MoveAreaCentre = centre;
            MoveAreaHalf = Mathf.Max(half, 0f);
        }

        // The selection map's cube: slides on the map plane, never resizes, no endpoints to keep out.
        public static ZoneConditions SlideOnPlane() =>
            new(true, false, null, true, true);

        public ZoneConditions WithMoveArea(Vector3 centre, float half) => new(this, centre, half);

        public Vector3 ClampToMoveArea(Vector3 centre)
        {
            if (!HasMoveArea)
                return centre;

            Vector3 min = MoveAreaCentre - Vector3.one * MoveAreaHalf;
            Vector3 max = MoveAreaCentre + Vector3.one * MoveAreaHalf;
            return new Vector3(
                Mathf.Clamp(centre.x, min.x, max.x),
                Mathf.Clamp(centre.y, min.y, max.y),
                Mathf.Clamp(centre.z, min.z, max.z));
        }
    }
}
