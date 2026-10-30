using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleBrief
    {
        public float EndpointClearance;
        public bool Translatable;
        public int Zones;
        public bool Resizable;
        public int Ships;

        public bool SupportsZones =>
            Zones <= 3;

        public bool IsMultiShip =>
            Ships > 1;

        public static PuzzleBrief From(PuzzleSeed seed, SpaceShapingSettings settings) => new()
        {
            EndpointClearance = settings.endpointClearance,
            Translatable = seed.translatable,
            Zones = Mathf.Max(1, seed.zones),
            Resizable = seed.resizable,
            Ships = Mathf.Max(1, seed.ships)
        };
    }
}
