using TMPro;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapSelectionReadout
    {
        private readonly MapSelectionLabel label;
        private readonly JobCardPanel card;

        public int PreviewIndex => label.PreviewIndex;

        public MapSelectionReadout(TMP_Text label, JobCardPanel card)
        {
            this.label = new MapSelectionLabel(label);
            this.card = card;
        }

        public void Show(JobEntry entry)
        {
            label.Show(entry);
            card.Show(entry.Puzzle, entry.State != JobState.Locked);
        }

        public void ShowPreview(JobEntry entry)
        {
            label.Show(entry);
            card.ShowInstant(entry.Puzzle, entry.State != JobState.Locked);
        }

        public void Reset()
        {
            label.Reset();
            card.Hide();
        }
    }
}
