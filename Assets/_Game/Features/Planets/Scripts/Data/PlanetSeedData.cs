using UnityEngine;
using DigitalLove.Global;

namespace DigitalLove.Game.Planets
{
    [CreateAssetMenu(fileName = "PlanetSeedData", menuName = "DigitalLove/Game/PlanetSeedData")]
    public class PlanetSeedData : ScriptableObject
    {
        public MinMaxFloat radius;
        public FloatValue maxDistanceBetweenPlanets;
        public MinMaxInt lettersPerMinute;
        public MinMaxInt maxLetters;
    }
}