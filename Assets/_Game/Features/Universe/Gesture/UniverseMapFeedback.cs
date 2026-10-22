namespace DigitalLove.Game.Universe
{
    internal class UniverseMapFeedback
    {
        public bool IsActive { get; private set; }
        public float ScaleFeedback { get; private set; }

        public void SetActive(bool active) => IsActive = active;

        public void SetScaleFeedback(float value) => ScaleFeedback = value;

        public void Reset()
        {
            IsActive = false;
            ScaleFeedback = 0f;
        }
    }
}
