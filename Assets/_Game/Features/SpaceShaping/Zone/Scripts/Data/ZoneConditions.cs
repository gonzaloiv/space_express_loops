using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public readonly struct ZoneConditions
    {
        public bool Translatable { get; }

        public bool Resizable { get; }

        public Vector3 Origin { get; }

        public Vector3 Destination { get; }

        public ZoneConditions(bool translatable, bool resizable)
            : this(translatable, resizable, Vector3.zero, Vector3.zero)
        {
        }

        public ZoneConditions(bool translatable, bool resizable, Vector3 origin, Vector3 destination)
        {
            Translatable = translatable;
            Resizable = resizable;
            Origin = origin;
            Destination = destination;
        }
    }
}
