using DigitalLove.Casual.Levels;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapSelectionReadout
    {
        private readonly JobCardPanel card;
        private readonly LevelSelector levels;

        public int PreviewIndex { get; private set; } = -1;

        public MapSelectionReadout(JobCardPanel card, LevelSelector levels)
        {
            this.card = card;
            this.levels = levels;
        }

        public void Show(JobEntry entry)
        {
            PreviewIndex = entry.Index;
            card.Show(
                entry.Puzzle, entry.State != JobState.Locked,
                levels.GetLevelOrdinal(entry.Puzzle));
        }

        public void ShowPreview(JobEntry entry)
        {
            PreviewIndex = entry.Index;
            card.ShowInstant(
                entry.Puzzle, entry.State != JobState.Locked,
                levels.GetLevelOrdinal(entry.Puzzle));
        }

        public void Reset()
        {
            PreviewIndex = -1;
            card.Hide();
        }
    }
}
