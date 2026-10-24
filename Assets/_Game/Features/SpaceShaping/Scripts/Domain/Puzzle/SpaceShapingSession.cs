using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingSession
    {
        private readonly SpaceShapingSetup setup;
        private readonly Vector3[] positions;
        private readonly float[] scales;
        private readonly SourceSpec[] specs;

        public SpaceShapingSession(SpaceShapingSetup setup)
        {
            this.setup = setup;
            positions = new Vector3[setup.Sources.Length];
            scales = new float[setup.Sources.Length];
            specs = new SourceSpec[setup.Sources.Length];
            Reset();
        }

        public int SourceCount => positions.Length;

        public Vector3 Origin => setup.Origin;

        public Vector3 Destination => setup.Destination;

        public Vector3 GetPosition(int index) => positions[index];

        public float GetScale(int index) => scales[index];

        public bool IsSolved(TrajectoryResult result) =>
            result.Outcome == TrajectoryOutcome.Arrive
            && result.ShipTime >= setup.Tmin
            && result.ShipTime <= setup.Tmax;

        public int Grade(float entropyLeft) =>
            entropyLeft >= setup.EntropyPar ? 3 : entropyLeft >= 0f ? 2 : 1;

        public void Reset()
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = setup.Sources[i].Home;
                scales[i] = setup.Sources[i].InitialScale;
                specs[i] = setup.Sources[i];
            }
        }

        public void ApplyZone(SpaceZone zone)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                SourceSpec source = setup.Sources[i];
                float factor = Mathf.Lerp(1f, zone.Scale, zone.InsideFactor(source.Home));
                positions[i] = zone.Centre + (source.Home - zone.Centre) * factor;
                scales[i] = source.InitialScale * factor;
                specs[i] = Scaled(source, zone.Centre, factor);
            }
        }

        public void Constrain() => SourceConstraints.Apply(positions, scales, specs, setup.SeparationScale);

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

        private static SourceSpec Scaled(SourceSpec source, Vector3 centre, float factor) => new()
        {
            Home = centre + (source.Home - centre) * factor,
            InitialScale = source.InitialScale * factor,
            MoveRange = source.MoveRange * factor,
            MinScale = source.MinScale * factor,
            MaxScale = source.MaxScale * factor
        };
    }
}
