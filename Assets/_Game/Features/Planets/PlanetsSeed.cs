using System;
using DigitalLove.Global;

namespace DigitalLove.Game.Planets
{
    [Serializable]
    public class PlanetsSeed
    {
        public MinMaxInt count;
        public PlanetSeed planetSeed;
    }

    [Serializable]
    public class PlanetSeed
    {
        public MinMaxFloat radius;
        public MinMaxFloat distanceToBase;
        public MinMaxInt lettersPerMinute;
    }
}