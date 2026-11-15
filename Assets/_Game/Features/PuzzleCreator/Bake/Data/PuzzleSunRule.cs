namespace DigitalLove.Game.SpaceShaping.Creator
{
    public enum PuzzleSunRule
    {
        Any,
        InZone,
        CentredGrown,
        OutOfZone,
        // OutOfZone, and no cube on the sun solves the job either: the sun is never an answer.
        NoSunAnswer
    }
}
