using System;

namespace DigitalLove.Game.PuzzleSelection
{
    public sealed class MapPanWatch
    {
        private readonly float promptSecs;
        private float idleSecs;
        private bool gestureActive;
        private bool due;

        public MapPanWatch(float promptSecs) => this.promptSecs = promptSecs;

        public event Action Due;

        public event Action Panned;

        internal void Reset()
        {
            idleSecs = 0f;
            gestureActive = false;
            due = false;
        }

        internal void SetGestureActive(bool active)
        {
            gestureActive = active;
            if (active)
                idleSecs = 0f;
        }

        internal void Tick(float deltaSecs, bool mapVisible)
        {
            if (!mapVisible)
            {
                Reset();
                return;
            }

            if (due)
                return;

            if (gestureActive)
            {
                idleSecs = 0f;
                return;
            }

            idleSecs += deltaSecs;
            if (idleSecs < promptSecs)
                return;

            due = true;
            Due?.Invoke();
        }

        internal void NotifyPanned() => Panned?.Invoke();
    }
}
