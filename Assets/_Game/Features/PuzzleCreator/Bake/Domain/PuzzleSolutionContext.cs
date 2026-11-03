using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public sealed class PuzzleSolutionContext
    {
        public readonly PuzzleData Puzzle;
        public readonly PuzzleSimSettings Settings;
        public readonly SpaceShapingSetup Setup;
        public readonly bool[] Obstacles;
        public readonly PuzzleMechanism Requested;

        private PuzzleSolutionContext(
            PuzzleData puzzle, PuzzleSimSettings settings, SpaceShapingSetup setup,
            bool[] obstacles, PuzzleMechanism requested)
        {
            Puzzle = puzzle;
            Settings = settings;
            Setup = setup;
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
                puzzle, settings, setup, obstacles, requested);
        }

        public PuzzleSolutionKind Classify(
            SpaceShapingSession solved, TrajectoryResult solvedResult) =>
            PuzzleSolutionClassifier.Classify(
                solved, Setup, Obstacles, new[] { solvedResult });

        public bool Matches(PuzzleSolutionKind kind) =>
            PuzzleSolutionClassifier.Matches(Requested, kind);
    }
}
