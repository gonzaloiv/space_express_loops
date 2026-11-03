using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipLaunch
    {
        public float LaunchSpeed = 1f;

        public TrajectoryResult Simulate(
            SpacetimeField field, Vector3 origin, Vector3 destination, TrajectorySettings settings, TrajectoryIntegrator integrator)
        {
            LaunchSettings launch = new()
            {
                Direction = (destination - origin).normalized,
                Speed = LaunchSpeed
            };

            return integrator.Integrate(field, origin, launch, settings);
        }
    }
}
