using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipLaunch
    {
        public float LaunchSpeed = 1f;

        public TrajectoryResult Simulate(
            SpacetimeField field, Vector3 origin, Vector3 destination, TrajectorySettings settings, TrajectoryIntegrator integrator) =>
            integrator.Integrate(field, origin, BuildLaunch(origin, destination), settings);

        public LaunchSettings BuildLaunch(Vector3 origin, Vector3 destination) => new()
        {
            Direction = (destination - origin).normalized,
            Speed = LaunchSpeed
        };
    }
}
