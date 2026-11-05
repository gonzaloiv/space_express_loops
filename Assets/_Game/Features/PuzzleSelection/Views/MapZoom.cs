using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapZoom
    {
        private readonly RunSlotView slot;
        private readonly SelectionCubeLens cube;
        private readonly float maxZoom;
        private bool wasZooming;

        public MapZoom(RunSlotView slot, SelectionCubeLens cube, float maxZoom)
        {
            this.slot = slot;
            this.cube = cube;
            this.maxZoom = maxZoom;
        }

        public void Update(bool isZooming, JobState state, float progress)
        {
            if (isZooming)
                ApplyScale(state, Mathf.Lerp(1f, maxZoom, progress));
            else if (wasZooming)
                ApplyScale(state, 1f);

            wasZooming = isZooming;
        }

        public void ApplyScale(JobState state, float scale)
        {
            slot.SetZoom(scale);
            cube.Show(state, scale);
        }

        public void Preview(JobState state) => cube.ShowState(state);

        public void Reset(JobState state) => ApplyScale(state, 1f);

        public void Clear()
        {
            slot.SetZoom(1f);
            cube.Reset();
            wasZooming = false;
        }
    }
}
