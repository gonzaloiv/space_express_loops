namespace DigitalLove.Game.SpaceShaping
{
    internal class PreviewClock
    {
        private const float PreviewIntervalSecs = 0.075f;
        private const float LatticeIntervalSecs = 1f / 30f;
        private const float ReadoutIntervalSecs = 0.1f;

        private float nextPreviewTime;
        private float nextLatticeTime;
        private float nextReadoutTime;

        public void Reset()
        {
            nextPreviewTime = 0f;
            nextLatticeTime = 0f;
            nextReadoutTime = 0f;
        }

        public bool TakePreview(float now) => Take(ref nextPreviewTime, now, PreviewIntervalSecs);

        public bool TakeLattice(float now) => Take(ref nextLatticeTime, now, LatticeIntervalSecs);

        public bool TakeReadout(float now) => Take(ref nextReadoutTime, now, ReadoutIntervalSecs);

        private static bool Take(ref float next, float now, float intervalSecs)
        {
            if (now < next)
                return false;

            next = now + intervalSecs;
            return true;
        }
    }
}
