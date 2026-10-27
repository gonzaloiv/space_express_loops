using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingSession
    {
        private const float ThreeStarUse = 1f / 3f;
        private const float TwoStarUse = 2f / 3f;

        private readonly SpaceShapingSetup setup;
        private readonly Vector3[] positions;
        private readonly float[] scales;
        private readonly SourceSpec[] specs;
        private readonly List<SpaceScaleCommand> commands = new();
        private readonly List<SpaceZone> zones = new();

        private SpaceScaleCommand preview;
        private float previewLastScale;
        private float entropySpent;

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

        public Vector3 GetPosition(int index) => positions[index];

        public float GetScale(int index) => scales[index];

        public bool IsSolved(TrajectoryResult result) =>
            result.Outcome == TrajectoryOutcome.Arrive
            && result.ShipTime >= setup.Tmin
            && result.ShipTime <= setup.Tmax;

        public int Grade()
        {
            float fraction = setup.EntropyBudget > 0f ? entropySpent / setup.EntropyBudget : 0f;
            if (fraction <= ThreeStarUse)
                return 3;

            return fraction <= TwoStarUse ? 2 : 1;
        }

        public void Reset()
        {
            commands.Clear();
            preview = null;
            previewLastScale = 0f;
            Recompute();
        }

        public void ApplyZone(SpaceZone zone)
        {
            PreviewZone(zone);
            CommitPreview();
        }

        public void PreviewZone(SpaceZone zone)
        {
            float reference = preview != null ? previewLastScale : LastCommittedScale();
            float cost = (preview != null ? preview.EntropyCost : 0f) + Mathf.Abs(zone.Scale - reference);
            previewLastScale = zone.Scale;
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
            return launch.Simulate(field, setup.Origin, setup.Destination, settings, integrator);
        }

        private void Recompute()
        {
            RestorePristine();
            ApplyCommands();
            SourceConstraints.Apply(positions, scales, specs, setup.Sim.SeparationScale);
            RebuildZones();
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

        private void ApplyCommands()
        {
            for (int c = 0; c < commands.Count; c++)
                commands[c].Apply(positions, scales, specs);

            if (preview != null)
                preview.Apply(positions, scales, specs);
        }

        private void RebuildZones()
        {
            zones.Clear();
            for (int c = 0; c < commands.Count; c++)
                zones.Add(commands[c].Zone);

            if (preview != null)
                zones.Add(preview.Zone);
        }

        private void RecomputeEntropy()
        {
            float total = preview != null ? preview.EntropyCost : 0f;
            for (int c = 0; c < commands.Count; c++)
                total += commands[c].EntropyCost;

            entropySpent = total;
        }

        private float LastCommittedScale() =>
            commands.Count > 0 ? commands[commands.Count - 1].Zone.Scale : 1f;

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
