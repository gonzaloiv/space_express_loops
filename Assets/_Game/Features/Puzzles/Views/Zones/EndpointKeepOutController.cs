using DigitalLove.Game.SpaceShaping.Cube;

namespace DigitalLove.Game.SpaceShaping
{
    // Drives the keep-out ghosts from the cube's move gesture: visible while moving, sized from
    // the cube's current box, and flashing the endpoint a refused drop hit.
    public sealed class EndpointKeepOutController
    {
        private readonly EndpointKeepOutPool pool;
        private readonly ZoneCubePool zones;
        private ZoneConditions conditions;
        private bool visible;

        public EndpointKeepOutController(EndpointKeepOutPool pool, ZoneCubePool zones)
        {
            this.pool = pool;
            this.zones = zones;
        }

        public void SetConditions(ZoneConditions value)
        {
            conditions = value;
            if (visible)
                Refresh();
        }

        public void Enable()
        {
            zones.MoveStarted += OnMoveStarted;
            zones.MoveEnded += OnMoveEnded;
            zones.OutOfBoundsAt += OnOutOfBoundsAt;
        }

        public void Disable()
        {
            zones.MoveStarted -= OnMoveStarted;
            zones.MoveEnded -= OnMoveEnded;
            zones.OutOfBoundsAt -= OnOutOfBoundsAt;
            Hide();
        }

        public void Tick()
        {
            if (visible)
                Refresh();
        }

        private void OnMoveStarted()
        {
            visible = true;
            Refresh();
        }

        private void OnMoveEnded() => Hide();

        private void OnOutOfBoundsAt(int endpoint) => pool.Flash(endpoint);

        private void Hide()
        {
            visible = false;
            pool.Hide();
        }

        private void Refresh()
        {
            if (conditions.IgnoresEndpoints || conditions.Endpoints == null)
            {
                pool.Hide();
                return;
            }

            pool.Show(conditions.Endpoints, zones.KeepOutSize);
        }
    }
}
