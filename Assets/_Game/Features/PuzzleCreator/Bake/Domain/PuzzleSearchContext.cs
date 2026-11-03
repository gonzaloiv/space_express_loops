using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public sealed class PuzzleSearchContext
    {
        public readonly PuzzleSolutionContext Solution;
        public readonly Vector3[] Centres;
        public readonly float[] Scales;
        public int Evaluations;

        public PuzzleSearchContext(PuzzleSolutionContext solution, Vector3[] centres, float[] scales)
        {
            Solution = solution;
            Centres = centres;
            Scales = scales;
        }

        public PuzzleData Puzzle => Solution.Puzzle;

        public PuzzleSimSettings Settings => Solution.Settings;
    }
}
