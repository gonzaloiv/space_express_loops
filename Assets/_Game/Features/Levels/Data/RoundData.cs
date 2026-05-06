using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    [CreateAssetMenu(fileName = "RoundData", menuName = "DigitalLove/Game/RoundData")]
    public class RoundData : Casual.Levels.LevelData
    {
        [Header("RoundData")]
        public int lettersToComplete;
        public PlanetsSeed planetsSeed;
        public SpaceshipSeed spaceshipSeed;
    }
}