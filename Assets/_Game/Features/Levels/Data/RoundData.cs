using DigitalLove.Game.Planets;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    [CreateAssetMenu(fileName = "RoundData", menuName = "DigitalLove/Game/RoundData")]
    public class RoundData : Casual.Levels.LevelData
    {
        [Header("RoundData")]
        public PlanetsSeed planetsSeed;
        public int lettersToComplete;
        public bool newSpaceship;
    }
}