using TMPro;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapSelectionLabel
    {
        private readonly TMP_Text label;

        public int PreviewIndex { get; private set; } = -1;

        public MapSelectionLabel(TMP_Text label)
        {
            this.label = label;
        }

        public void Show(JobEntry entry)
        {
            PreviewIndex = entry.Index;
            label.text = Format(entry.State, entry.Stars, entry.NoGrade);
        }

        public void Reset() => PreviewIndex = -1;

        private static string Format(JobState state, int stars, bool noGrade)
        {
            switch (state)
            {
                case JobState.Solved:
                    return noGrade ? "solved" : "solved · " + stars + "/3";
                case JobState.Current:
                    return "next";
                default:
                    return "locked";
            }
        }
    }
}