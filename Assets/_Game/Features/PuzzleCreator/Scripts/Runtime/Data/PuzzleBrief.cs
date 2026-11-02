using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleBrief
    {
        public float EndpointClearance;
        public float SunRadius;
        public bool SunScalable;
        public bool Translatable;
        public int Zones;
        public bool Resizable;
        public int Ships;
        public PuzzleShipRelation Relation;
        public bool NoFail;
        public bool NoGrade;

        public bool SupportsZones =>
            Zones <= 3;

        public bool IsMulti =>
            Ships > 1;

        public static PuzzleBrief From(PuzzleSeed seed, SpaceShapingSettings settings) => new()
        {
            EndpointClearance = settings.endpointClearance,
            SunRadius = seed.EffectiveSunRadius(settings),
            SunScalable = true,
            Translatable = seed.translatable,
            Zones = Mathf.Max(1, seed.zones),
            Resizable = seed.resizable,
            Ships = Mathf.Max(1, seed.ships),
            Relation = seed.relation,
            NoFail = seed.noFail,
            NoGrade = seed.noGrade
        };
    }
}
