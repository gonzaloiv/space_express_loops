namespace DigitalLove.Game.SpaceShaping
{
    public readonly struct ZoneConditions
    {
        public bool Translatable { get; }

        public bool Resizable { get; }

        public ZoneConditions(bool translatable, bool resizable)
        {
            Translatable = translatable;
            Resizable = resizable;
        }

        public static ZoneConditions From(PuzzleData puzzle) =>
            new(puzzle.translatable, puzzle.resizable);
    }
}
