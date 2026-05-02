using System;

namespace DigitalLove.Game.Spaceships
{
    [Serializable]
    public class SpaceshipSeed
    {
        public int percentageToSpawn;
        public bool inBase;

        public bool ShouldSpawn => UnityEngine.Random.Range(0, 100) < percentageToSpawn;
    }
}
