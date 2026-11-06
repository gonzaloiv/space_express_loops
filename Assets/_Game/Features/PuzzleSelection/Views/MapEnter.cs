using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapEnter
    {
        private readonly MapZoom zoom;
        private readonly float secs;
        private readonly float targetScale;
        private bool active;
        private float elapsed;
        private float startScale;

        public bool IsActive => active;

        public MapEnter(MapZoom zoom, float secs, float targetScale)
        {
            this.zoom = zoom;
            this.secs = Mathf.Max(secs, 0.0001f);
            this.targetScale = targetScale;
        }

        public void Start(float scale)
        {
            active = true;
            elapsed = 0f;
            startScale = scale;
        }

        public bool Tick(JobState state, float deltaSecs)
        {
            if (!active)
                return false;

            elapsed += deltaSecs;
            float t = Mathf.Clamp01(elapsed / secs);
            zoom.ApplyScale(state, Mathf.Lerp(startScale, targetScale, t));
            if (t < 1f)
                return false;

            active = false;
            return true;
        }

        public void Cancel(JobState state)
        {
            if (!active)
                return;

            active = false;
            zoom.Update(false, state, 0f);
        }

        public void Reset() => active = false;
    }
}
