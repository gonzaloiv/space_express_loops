namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A retry must not land on the next seed's field: attempt 1 of seed 5 is not seed 6. One salted
    // mix, used by the field, the lesson and the decoy step.
    public static class PuzzleConstructSeeds
    {
        public const int RetrySalt = 7919;

        public static int Value(int seed, int attempt) => seed + attempt * RetrySalt;
    }
}
