namespace DigitalLove.Game.SpaceShaping.Cube
{
    // The result of placing a frame: allowed, or the endpoint index it would swallow.
    public readonly struct ZonePlacement
    {
        private const int NoEndpoint = -1;

        public bool Allowed { get; }

        public int Endpoint { get; }

        public bool Blocked => !Allowed;

        private ZonePlacement(bool allowed, int endpoint)
        {
            Allowed = allowed;
            Endpoint = endpoint;
        }

        public static ZonePlacement Ok() => new(true, NoEndpoint);

        public static ZonePlacement BlockedBy(int endpoint) => new(false, endpoint);
    }
}
