using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PuzzlePlanets", menuName = "DigitalLove/Game/Puzzle Planets")]
    public class PuzzlePlanetListAsset : ScriptableObject
    {
        public float distance = 8f;
        public PuzzleFieldPlanet[] planets = Array.Empty<PuzzleFieldPlanet>();
    }
}
