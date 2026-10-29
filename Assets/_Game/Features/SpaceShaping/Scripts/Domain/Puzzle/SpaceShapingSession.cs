using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingSession
    {
        private readonly SpaceShapingSetup setup;
        private readonly Vector3[] positions;
        private readonly float[] scales;
        private readonly SourceSpec[] specs;
        private readonly List<SpaceScaleCommand> commands = new();
        private readonly List<SpaceZone> zones = new();

        private SpaceScaleCommand preview;
        private float entropySpent;
        private float entropyCommitted;

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

        public IReadOnlyList<SpaceZone> Zones => zones;

        public int CommandCount => commands.Count;

        public float EntropySpent => entropySpent;

        public float EntropyCommitted => entropyCommitted;

        public float PendingEntropy => entropySpent - entropyCommitted;

        public float EntropyBudget => setup.EntropyBudget;

        public float Tmin => setup.Tmin;

        public float Tmax => setup.Tmax;

        public bool IsOverBudget => setup.EntropyBudget > 0f && entropyCommitted >= setup.EntropyBudget;

        public Vector3 GetPosition(int index) => positions[index];

        public float GetScale(int index) => scales[index];

        public bool IsSolved(TrajectoryResult result) =>
            result.Outcome == TrajectoryOutcome.Arrive
            && result.ShipTime >= setup.Tmin
            && result.ShipTime <= setup.Tmax;

        public bool IsFailed(TrajectoryResult result) => IsOverBudget && !IsSolved(result);

        public int Grade() => PuzzleGrade.Stars(entropyCommitted, setup.EntropyBudget);

        public void Reset()
        {
            commands.Clear();
            preview = null;
            Recompute();
        }

        public void ApplyZone(SpaceZone zone)
        {
            PreviewZone(zone);
            CommitPreview();
        }

        public void PreviewZone(SpaceZone zone)
        {
            float cost = Mathf.Abs(zone.Scale - ReferenceScale(zone));
            preview = new SpaceScaleCommand(zone, cost);
            Recompute();
        }

        public void CommitPreview()
        {
            if (preview == null)
                return;

            commands.Add(preview);
            preview = null;
            Recompute();
        }

        public bool Undo()
        {
            if (commands.Count == 0)
                return false;

            commands.RemoveAt(commands.Count - 1);
            Recompute();
            return true;
        }

        public SpacetimeField BuildField()
        {
            SpacetimeField field = new();
            for (int i = 0; i < positions.Length; i++)
            {
                float radius = scales[i] * 0.5f;
                float mass = radius * radius * radius;
                field.Add(new SpaceSource(positions[i], radius, mass * setup.Sim.SourceStrengthScale));
            }

            return field;
        }

        public TrajectoryResult Simulate() => Simulate(BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

        public TrajectoryResult Simulate(
            SpacetimeField field, TrajectoryIntegrator integrator, ShipLaunch launch)
        {
            TrajectorySettings settings = new()
            {
                Destination = setup.Destination,
                CaptureRadius = setup.Sim.CaptureRadius,
                StepSecs = setup.Sim.StepSecs,
                MaxSteps = StepBudget(),
                LightSpeed = setup.Sim.LightSpeed,
                Zones = zones
            };

            launch.LaunchSpeed = setup.LaunchSpeed;
            launch.LaunchAngleDegs = setup.LaunchAngleDegs;
            return launch.Simulate(field, setup.Origin, setup.Destination, settings, integrator);
        }

        private void Recompute()
        {
            RestorePristine();
            RebuildZones();
            ApplyZones();
            SourceConstraints.Apply(positions, scales, specs, setup.Sim.SeparationScale);
            RecomputeEntropy();
        }

        private void RestorePristine()
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = setup.Sources[i].Home;
                scales[i] = setup.Sources[i].InitialScale;
                specs[i] = setup.Sources[i];
            }
        }

        private void RebuildZones()
        {
            zones.Clear();
            for (int c = 0; c < commands.Count; c++)
                UpsertZone(commands[c].Zone);

            if (preview != null)
                UpsertZone(preview.Zone);
        }

        private void UpsertZone(SpaceZone zone)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (!zones[i].SameRegion(zone))
                    continue;

                zones[i] = zone;
                return;
            }

            zones.Add(zone);
        }

        private void ApplyZones()
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].ApplyTo(positions, scales, specs);
        }

        private void RecomputeEntropy()
        {
            float committed = 0f;
            for (int c = 0; c < commands.Count; c++)
                committed += commands[c].EntropyCost;

            entropyCommitted = committed;
            entropySpent = committed + (preview != null ? preview.EntropyCost : 0f);
        }

        private float ReferenceScale(SpaceZone zone)
        {
            for (int c = commands.Count - 1; c >= 0; c--)
            {
                if (commands[c].Zone.SameRegion(zone))
                    return commands[c].Zone.Scale;
            }

            return 1f;
        }

        private int StepBudget()
        {
            float factor = 1f;
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].IsActive)
                    factor = Mathf.Max(factor, zones[i].Scale);
            }

            return Mathf.CeilToInt(setup.MaxSteps * factor);
        }
    }
}
