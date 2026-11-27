using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Builds the stored answer from the bake's solution, with the numbers the bake report pins.
    public static class PuzzleAnswerFactory
    {
        public static PuzzleAnswerAsset From(
            PuzzleJobContract job, int seed, PuzzleData puzzle, PuzzleSolution solution,
            PuzzleSimSettings sim)
        {
            PuzzleAnswerAsset answer = ScriptableObject.CreateInstance<PuzzleAnswerAsset>();
            answer.id = job.Id;
            answer.kind = solution.Kind;
            answer.zones = solution.Zones ?? System.Array.Empty<PuzzleZone>();
            answer.seed = seed;
            answer.window = PuzzleSolveWindow.Measure(puzzle, solution.Zones, sim);
            MeasureStartingFrame(answer, puzzle, solution.Zones);
            return answer;
        }

        private static void MeasureStartingFrame(
            PuzzleAnswerAsset answer, PuzzleData puzzle, PuzzleZone[] zones)
        {
            if (zones == null || zones.Length == 0)
                return;

            answer.scaleDelta = Mathf.Abs(zones[0].scale - 1f);
            if (puzzle.frames == null || puzzle.frames.Length == 0)
                return;

            answer.startDistance = Vector3.Distance(puzzle.frames[0].centre, zones[0].centre);
        }
    }
}
