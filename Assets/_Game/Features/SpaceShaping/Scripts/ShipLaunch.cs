using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipLaunch
    {
        public LaunchModel Model = LaunchModel.Coast;
        public float LaunchSpeed = 1f;
        public float ThrustAcceleration = 1f;

        public TrajectoryResult Simulate(
            SpacetimeField field, Vector3 origin, Vector3 destination, TrajectorySettings settings, TrajectoryIntegrator integrator)
        {
            Vector3 direction = (destination - origin).normalized;
            LaunchSettings launch = new()
            {
                Model = Model,
                Direction = direction,
                Speed = LaunchSpeed,
                ThrustAcceleration = ThrustAcceleration
            };

            return integrator.Integrate(field, origin, launch, settings);
        }
    }
}
