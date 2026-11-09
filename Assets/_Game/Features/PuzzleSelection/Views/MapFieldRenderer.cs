using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapFieldRenderer
    {
        private readonly RunSlotView selectedSlot;
        private readonly ViewPool<RunSlotView> fields;
        private readonly MapNodeTint tint;
        private readonly MapSelectionReadout readout;
        private readonly bool showFrames;

        public RunSlotView CurrentSlot { get; private set; }

        public MapFieldRenderer(
            RunSlotView selectedSlot, ViewPool<RunSlotView> fields,
            MapNodeTint tint, MapSelectionReadout readout, bool showFrames)
        {
            this.selectedSlot = selectedSlot;
            this.fields = fields;
            this.tint = tint;
            this.readout = readout;
            this.showFrames = showFrames;
        }

        public void Render(PuzzleMap map)
        {
            CurrentSlot = null;
            RenderSelected(map);
            RenderNeighbours(map);
        }

        public void TickPulse(float deltaSecs)
        {
            if (CurrentSlot != null)
                CurrentSlot.SetLabelColor(tint.CurrentAt(deltaSecs));
        }

        public void PreviewNearest(PuzzleMap map, Transform mapRoot, Vector3 pointer)
        {
            int nearest = MapSnap.Nearest(map, mapRoot, pointer);
            if (nearest < 0 || nearest == readout.PreviewIndex)
                return;

            JobEntry entry = map[nearest];
            readout.ShowPreview(entry);
        }

        public void Hide()
        {
            CurrentSlot = null;
            selectedSlot.Hide();
            fields.ForEachFrom(0, field => field.Hide());
        }

        private void RenderSelected(PuzzleMap map)
        {
            JobEntry selected = map.Selected;
            selectedSlot.ShowMapField(selected, tint.For(selected.State), showFrames);
            readout.Show(selected);
            selectedSlot.SetZoom(1f);
            if (selected.State == JobState.Current)
                CurrentSlot = selectedSlot;
        }

        private void RenderNeighbours(PuzzleMap map)
        {
            int slot = 0;
            for (int i = 0; i < map.Count; i++)
            {
                if (i == map.SelectedIndex)
                    continue;

                JobEntry entry = map[i];
                RunSlotView field = fields.EnsureSlot(slot++);
                field.ShowMapField(entry, tint.For(entry.State), showFrames);
                field.PlaceSunAt(map.Position(i));
                if (entry.State == JobState.Current)
                    CurrentSlot = field;
            }

            fields.ForEachFrom(slot, field => field.Hide());
        }
    }
}