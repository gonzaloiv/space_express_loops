using UnityEngine;
using DigitalLove.Global;

namespace DigitalLove.Game.Planets
{
    [CreateAssetMenu(fileName = "PlanetSeedData", menuName = "DigitalLove/Game/PlanetSeedData")]
    public class PlanetSeedData : ScriptableObject
    {
        public MinMaxFloat radius;
        public MinMaxFloat distanceToBase;
        public MinMaxInt lettersPerMinute;
        public MinMaxInt maxLetters;
    }
}