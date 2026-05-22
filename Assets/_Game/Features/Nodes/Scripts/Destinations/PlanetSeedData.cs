using UnityEngine;
using DigitalLove.Global;

namespace DigitalLove.Game.Nodes
{
    [CreateAssetMenu(fileName = "PlanetSeedData", menuName = "DigitalLove/Game/PlanetSeedData")]
    public class PlanetSeedData : ScriptableObject
    {
        public MinMaxFloat radius;
        public FloatValue maxDistanceToOtherPlanet;
        public MinMaxInt lettersPerMinute;
        public MinMaxFloat maxLettersMultiplier;
    }
}