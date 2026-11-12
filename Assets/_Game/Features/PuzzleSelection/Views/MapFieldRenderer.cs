using System;
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
        private MapEnding ending;

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
            ending?.Tick(deltaSecs);
            if (CurrentSlot != null)
                CurrentSlot.SetLabelColor(tint.CurrentAt(deltaSecs));
        }

        public void PlayEnding(float nodeSecs, float finishSecs, Action onComplete)
        {
            // The run's ending walks every job, so every number comes back.
            selectedSlot.SetNumberShown(true);
            fields.ForEachFrom(0, field => field.SetNumberShown(true));
            ending ??= new MapEnding(
                selectedSlot, fields, tint, nodeSecs, finishSecs);
            ending.Play(onComplete);
        }

        public void StopEnding() => ending?.Stop();

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
            ending?.Stop();
            ending = null;
            CurrentSlot = null;
            selectedSlot.Hide();
            fields.ForEachFrom(0, field => field.Hide());
        }

        private void RenderSelected(PuzzleMap map)
        {
            JobEntry selected = map.Selected;
            selectedSlot.ShowMapField(selected, tint.For(selected.State), showFrames, true);
            selectedSlot.SetNumberShown(true);
            readout.Show(selected);
            selectedSlot.SetZoom(1f);
            if (selected.State == JobState.Current
                && map.SelectedIndex == map.HighestCurrentIndex)
                CurrentSlot = selectedSlot;
        }

        private void RenderNeighbours(PuzzleMap map)
        {
            int highest = map.HighestCurrentIndex;
            int slot = 0;
            for (int i = 0; i < map.Count; i++)
            {
                if (i == map.SelectedIndex)
                    continue;

                JobEntry entry = map[i];
                RunSlotView field = fields.EnsureSlot(slot++);
                field.ShowMapField(entry, tint.For(entry.State), showFrames, false);
                field.SetNumberShown(IsRouteNeighbour(map, i));
                field.PlaceSunAt(map.Position(i));
                if (entry.State == JobState.Current && i == highest)
                    CurrentSlot = field;
            }

            fields.ForEachFrom(slot, field => field.Hide());
        }

        // Only the jobs either side of the selected one along the route keep their number.
        private static bool IsRouteNeighbour(PuzzleMap map, int index) =>
            Mathf.Abs(index - map.SelectedIndex) == 1;
    }
}