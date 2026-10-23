using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingSession
    {
        private readonly SpaceShapingSetup setup;
        private readonly Vector3[] positions;
        private readonly float[] scales;

        public SpaceShapingSession(SpaceShapingSetup setup)
        {
            this.setup = setup;
            positions = new Vector3[setup.Sources.Length];
            scales = new float[setup.Sources.Length];
            Reset();
        }

        public int SourceCount => positions.Length;

        public Vector3 Origin => setup.Origin;

        public Vector3 Destination => setup.Destination;

        public Vector3 GetPosition(int index) => positions[index];

        public float GetScale(int index) => scales[index];

        public bool IsSolved(TrajectoryResult result) =>
            result.Outcome == TrajectoryOutcome.Arrive && result.ShipTime <= setup.Deadline;

        public void SetSource(int index, Vector3 position, float scale)
        {
            positions[index] = position;
            scales[index] = scale;
        }

        public void Reset()
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = setup.Sources[i].Home;
                scales[i] = setup.Sources[i].InitialScale;
            }
        }

        public void Constrain() => SourceConstraints.Apply(positions, scales, setup.Sources, setup.SeparationScale);

        public SpacetimeField BuildField()
        {
            SpacetimeField field = new();
            for (int i = 0; i < positions.Length; i++)
            {
                float radius = scales[i] * 0.5f;
                field.Add(new SpaceSource(positions[i], radius, radius * setup.SourceStrengthScale));
            }

            return field;
        }

        public TrajectoryResult Simulate(
            SpacetimeField field, TrajectoryIntegrator integrator, ShipLaunch launch, SpaceZone zone)
        {
            TrajectorySettings settings = new()
            {
                Destination = setup.Destination,
                CaptureRadius = setup.CaptureRadius,
                StepSecs = setup.StepSecs,
                MaxSteps = StepBudget(zone),
                DilationScale = setup.DilationScale,
                Zone = zone
            };

            launch.Model = setup.LaunchModel;
            launch.LaunchSpeed = setup.LaunchSpeed;
            launch.ThrustAcceleration = setup.ThrustAcceleration;
            return launch.Simulate(field, setup.Origin, setup.Destination, settings, integrator);
        }

        private int StepBudget(SpaceZone zone) =>
            zone.IsActive ? Mathf.CeilToInt(setup.MaxSteps * Mathf.Max(1f, zone.Scale)) : setup.MaxSteps;
    }
}
