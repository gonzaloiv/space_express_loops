using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // Once the pinch passes the threshold the entry commits: the diorama keeps growing to the
    // target on its own, then the job is entered.
    internal class MapEnter
    {
        private readonly RunSlotView slot;
        private readonly float secs;
        private readonly float targetScale;
        private bool active;
        private float elapsed;
        private float startScale;

        public bool IsActive => active;

        public float Progress => Mathf.Clamp01(elapsed / secs);

        public MapEnter(RunSlotView slot, float secs, float targetScale)
        {
            this.slot = slot;
            this.secs = Mathf.Max(secs, 0.0001f);
            this.targetScale = targetScale;
        }

        public void Start(float scale)
        {
            active = true;
            elapsed = 0f;
            startScale = scale;
        }

        public bool Tick(float deltaSecs)
        {
            if (!active)
                return false;

            elapsed += deltaSecs;
            float t = Mathf.Clamp01(elapsed / secs);
            slot.SetZoom(Mathf.Lerp(startScale, targetScale, t * t * (3f - 2f * t)));
            if (t < 1f)
                return false;

            active = false;
            return true;
        }

        public void Reset() => active = false;
    }
}
