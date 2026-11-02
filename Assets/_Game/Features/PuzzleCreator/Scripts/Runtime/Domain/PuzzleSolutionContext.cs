using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal sealed class PuzzleSolutionContext
    {
        public readonly PuzzleData Puzzle;
        public readonly PuzzleSimSettings Settings;
        public readonly SpaceShapingSetup Setup;
        public readonly TrajectoryResult[] StartResults;
        public readonly bool[] Obstacles;
        public readonly PuzzleMechanism Requested;

        private PuzzleSolutionContext(
            PuzzleData puzzle, PuzzleSimSettings settings, SpaceShapingSetup setup,
            TrajectoryResult[] startResults, bool[] obstacles, PuzzleMechanism requested)
        {
            Puzzle = puzzle;
            Settings = settings;
            Setup = setup;
            StartResults = startResults;
            Obstacles = obstacles;
            Requested = requested;
        }

        public static PuzzleSolutionContext Create(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleMechanism requested)
        {
            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, settings);
            SpaceShapingSession start = new(setup);
            TrajectoryResult[] startResults = start.SimulateAll();
            bool[] obstacles = PuzzleSolutionClassifier.Obstacles(setup, startResults);
            return new PuzzleSolutionContext(
                puzzle, settings, setup, startResults, obstacles, requested);
        }

        public PuzzleSolutionKind Classify(
            SpaceShapingSession solved, TrajectoryResult solvedResult) =>
            PuzzleSolutionClassifier.Classify(
                solved, Setup, Obstacles, new[] { solvedResult });

        public bool Matches(PuzzleSolutionKind kind) =>
            PuzzleSolutionClassifier.Matches(Requested, kind);
    }
}
