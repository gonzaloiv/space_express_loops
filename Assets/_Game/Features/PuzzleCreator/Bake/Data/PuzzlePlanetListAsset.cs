using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PuzzlePlanets", menuName = "DigitalLove/Game/Puzzle Planets")]
    public class PuzzlePlanetListAsset : ScriptableObject
    {
        public PuzzleFieldPlanet[] planets = Array.Empty<PuzzleFieldPlanet>();

        [System.NonSerialized] public bool hasCandidate;
        [System.NonSerialized] public Vector3 candidateOrigin;
        [System.NonSerialized] public Vector3 candidateDestination;
        [System.NonSerialized] public int blockerIndex = -1;
        [System.NonSerialized] public int wellIndex = -1;
        [System.NonSerialized] public PuzzlePostClearReport postClear;
    }
}
