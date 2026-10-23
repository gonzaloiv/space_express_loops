using System;

namespace DigitalLove.Game.Sectors
{
    [Serializable]
    public class SectorFieldData
    {
        public int worldSeed = 1;
        public float cellSize = 5f;
        public float fieldRadius = 12.5f;
        public float density = 0.55f;
        public float gap = 0.5f;
        public float destinationRadius;
        public int layers = 1;
        public int softSectorBudget = 16;
        public int softPlanetBudget = 64;
        public SectorShapeData shape = new();
    }
}
