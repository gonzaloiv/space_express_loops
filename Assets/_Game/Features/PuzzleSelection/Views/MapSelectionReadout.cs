namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapSelectionReadout
    {
        private readonly JobCardPanel card;

        public int PreviewIndex { get; private set; } = -1;

        public MapSelectionReadout(JobCardPanel card)
        {
            this.card = card;
        }

        public void Show(JobEntry entry)
        {
            PreviewIndex = entry.Index;
            card.Show(entry.Puzzle, entry.State != JobState.Locked);
        }

        public void ShowPreview(JobEntry entry)
        {
            PreviewIndex = entry.Index;
            card.ShowInstant(entry.Puzzle, entry.State != JobState.Locked);
        }

        public void Reset()
        {
            PreviewIndex = -1;
            card.Hide();
        }
    }
}
