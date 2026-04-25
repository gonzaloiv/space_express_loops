using DigitalLove.Casual.Levels;
using DigitalLove.Game.Planets;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    [CreateAssetMenu(fileName = "RoundData", menuName = "DigitalLove/Game/RoundData")]
    public class RoundData : LevelData
    {
        [Header("RoundData")]
        public PlanetsSeed planetsSeed;
    }
}