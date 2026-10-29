using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipLaunch
    {
        public float LaunchSpeed = 1f;

        public float LaunchAngleDegs;

        public TrajectoryResult Simulate(
            SpacetimeField field, Vector3 origin, Vector3 destination, TrajectorySettings settings, TrajectoryIntegrator integrator)
        {
            LaunchSettings launch = new()
            {
                Direction = Direction(origin, destination),
                Speed = LaunchSpeed
            };

            return integrator.Integrate(field, origin, launch, settings);
        }

        private Vector3 Direction(Vector3 origin, Vector3 destination)
        {
            Vector3 straight = (destination - origin).normalized;
            return Mathf.Abs(LaunchAngleDegs) < 0.0001f
                ? straight
                : Quaternion.AngleAxis(LaunchAngleDegs, Vector3.up) * straight;
        }
    }
}
