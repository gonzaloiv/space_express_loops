using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.Flow
{
    // A one-shot intent written when the player exits a job to the map, and cleared when the map
    // shows: the map opens on the job they left, so restarting it is one pinch away.
    internal readonly struct MapReturn
    {
        public MapReturn(PuzzleData puzzle) => Puzzle = puzzle;

        public PuzzleData Puzzle { get; }
    }
}
