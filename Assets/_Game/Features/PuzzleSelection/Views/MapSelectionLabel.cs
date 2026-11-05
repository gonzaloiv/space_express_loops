namespace DigitalLove.Game.PuzzleSelection
{
    internal static class MapSelectionLabel
    {
        public static string Format(JobState state, int stars)
        {
            switch (state)
            {
                case JobState.Solved:
                    return "solved · " + stars + "/3";
                case JobState.Current:
                    return "next";
                default:
                    return "locked";
            }
        }
    }
}
