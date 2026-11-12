using DigitalLove.Game.Spatial;

namespace DigitalLove.Game.SpaceShaping
{
    // One red copy of the cube while a move is refused, and one danger pulse on the endpoint planet
    // when the refusal starts.
    public sealed class EndpointKeepOutController
    {
        private readonly EndpointKeepOutPool pool;
        private readonly ZoneCubePool zones;
        private readonly FieldPlanetSet planets;
        private ZoneConditions conditions;
        private bool visible;

        public EndpointKeepOutController(
            EndpointKeepOutPool pool, ZoneCubePool zones, FieldPlanetSet planets)
        {
            this.pool = pool;
            this.zones = zones;
            this.planets = planets;
        }

        public void SetConditions(ZoneConditions value) => conditions = value;

        public void Enable()
        {
            zones.MoveEnded += OnMoveEnded;
            zones.OutOfBoundsAt += OnOutOfBoundsAt;
        }

        public void Disable()
        {
            zones.MoveEnded -= OnMoveEnded;
            zones.OutOfBoundsAt -= OnOutOfBoundsAt;
            Hide();
        }

        public void Tick()
        {
            CubeMoveRefusal refusal = zones.MoveRefusal;
            if (!refusal.Blocked)
            {
                if (visible)
                    Hide();
                return;
            }

            pool.Show(refusal.Centre, refusal.Size, zones.Danger);
            visible = true;
        }

        private void OnMoveEnded() => Hide();

        private void OnOutOfBoundsAt(int endpoint)
        {
            if (conditions.Endpoints == null || endpoint < 0 || endpoint >= conditions.Endpoints.Count)
                return;

            planets.PulseEndpoint(conditions.Endpoints[endpoint], zones.Danger);
        }

        private void Hide()
        {
            visible = false;
            pool.Hide();
        }
    }
}
