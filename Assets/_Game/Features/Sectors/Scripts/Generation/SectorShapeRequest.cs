using DigitalLove.Global;

namespace DigitalLove.Game.Sectors
{
    internal struct SectorShapeRequest
    {
        public SectorShapeKind Kind;
        public float SectorRadius;
        public float AnchorRadius;
        public int PlanetCount;
        public float Spread;
        public float Inclination;
        public float MinSeparation;
        public MinMaxFloat PlanetRadius;
        public MinMaxInt RingCount;
        public MinMaxInt ArmCount;

        public float OuterRadius => SectorRadius * Spread;
    }
}
