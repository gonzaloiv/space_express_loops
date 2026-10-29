using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal sealed class PuzzleSearchContext
    {
        public readonly PuzzleData Puzzle;
        public readonly PuzzleSimSettings Settings;
        public readonly Vector3[] Centres;
        public readonly float[] Scales;
        public int Evaluations;

        public PuzzleSearchContext(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3[] centres, float[] scales)
        {
            Puzzle = puzzle;
            Settings = settings;
            Centres = centres;
            Scales = scales;
        }
    }
}
