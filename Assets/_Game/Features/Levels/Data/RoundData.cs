using DigitalLove.Game.Planets;
using UnityEngine;
using DigitalLove.Global;

namespace DigitalLove.Game.Levels
{
    [CreateAssetMenu(fileName = "RoundData", menuName = "DigitalLove/Game/RoundData")]
    public class RoundData : Casual.Levels.LevelData
    {
        [Header("RoundData")]
        public float lettersIncreaseMultiplier = 1f;
        public MinMaxInt planetsToAdd;
        public PlanetSeedData planetSeed;
        public bool shouldSpawnSpaceship;
    }
}