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
        private readonly ShipSpec[] ships;
        private readonly ZoneCommandLog log = new();

        public SpaceShapingSession(SpaceShapingSetup setup)
        {
            this.setup = setup;
            positions = new Vector3[setup.Sources.Length];
            scales = new float[setup.Sources.Length];
            specs = new SourceSpec[setup.Sources.Length];
            ships = ShipSpecs.Resolve(setup);
            Reset();
        }

        public int SourceCount => positions.Length;

        public int ShipCount => ships.Length;

        public ShipSpec Ship(int index) => ships[index];

        public Vector3 Origin => setup.Origin;

        public Vector3 Destination => setup.Destination;

        // The source sitting at a point, or -1 when none does.
        public int IndexOf(Vector3 point)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                if ((setup.Sources[i].Home - point).sqrMagnitude < 0.000001f)
                    return i;
            }

            return -1;
        }

        // The planet radius at a point (0 if no planet sits there). A source's scale is its diameter:
        // the sphere is scaled by the full source scale, matching the radius the field integrates with.
        public float RadiusAt(Vector3 point)
        {
            int index = IndexOf(point);
            return index >= 0 ? scales[index] * 0.5f : 0f;
        }

        public IReadOnlyList<SpaceZone> Zones => log.Zones;

        public int CommandCount => log.Count;

        public float EntropySpent => log.Spent;

        public float EntropyCommitted => log.Committed;

        public float PendingEntropy => log.Pending;

        public float EntropyBudget => setup.EntropyBudget;

        public bool IsOverBudget => setup.EntropyBudget > 0f && log.Committed >= setup.EntropyBudget;

        public Vector3 Home(int index) => setup.Sources[index].Home;

        public float HomeScale(int index) => setup.Sources[index].InitialScale;

        public Vector3 GetPosition(int index) => positions[index];

        public float GetScale(int index) => scales[index];

        public bool IsSolved(TrajectoryResult result) =>
            result.Outcome == TrajectoryOutcome.Arrive;

        public bool IsSolved(IReadOnlyList<TrajectoryResult> results) => results.AllArrived();

        public bool IsFailed(IReadOnlyList<TrajectoryResult> results) =>
            IsOverBudget && !results.AllArrived();

        public int Grade() => PuzzleGrade.Stars(log.Committed, setup.EntropyBudget);

        public void Reset()
        {
            log.Clear();
            Recompute();
        }

        public void ApplyZone(SpaceZone zone)
        {
            PreviewZone(zone);
            CommitPreview();
        }

        public void PreviewZone(SpaceZone zone)
        {
            log.Preview(zone);
            Recompute();
        }

        public void CommitPreview()
        {
            log.Commit();
            Recompute();
        }

        public bool Undo()
        {
            if (!log.Undo())
                return false;

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

        // The primary run: the first ship. A single-ship puzzle has exactly this one.
        public TrajectoryResult Simulate() => Simulate(BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

        public TrajectoryResult Simulate(
            SpacetimeField field, TrajectoryIntegrator integrator, ShipLaunch launch) =>
            SimulateShip(field, integrator, launch, ships[0], StepBudget());

        // One result per ship, all sharing the budget of the longest run.
        public TrajectoryResult[] SimulateAll() =>
            SimulateAll(BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

        public TrajectoryResult[] SimulateAll(
            SpacetimeField field, TrajectoryIntegrator integrator, ShipLaunch launch)
        {
            TrajectoryResult[] results = new TrajectoryResult[ships.Length];
            int budget = StepBudget();
            for (int i = 0; i < ships.Length; i++)
                results[i] = SimulateShip(field, integrator, launch, ships[i], budget);

            return results;
        }

        private TrajectoryResult SimulateShip(
            SpacetimeField field, TrajectoryIntegrator integrator, ShipLaunch launch,
            ShipSpec ship, int budget)
        {
            TrajectorySettings settings = new()
            {
                Destination = ship.Destination,
                CaptureRadius = ship.CaptureRadius > 0f ? ship.CaptureRadius : setup.Sim.CaptureRadius,
                StepSecs = setup.Sim.StepSecs,
                MaxSteps = budget,
                Zones = log.Zones
            };

            launch.LaunchSpeed = setup.LaunchSpeed;
            return launch.Simulate(field, ship.Origin, ship.Destination, settings, integrator);
        }

        private void Recompute()
        {
            RestorePristine();
            log.Rebuild(setup.Sim.ZoneEffectGain);
            ApplyZones();
            SourceConstraints.Apply(positions, scales, specs, setup.Sim.SeparationScale);
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

        private void ApplyZones()
        {
            IReadOnlyList<SpaceZone> zones = log.Zones;
            for (int i = 0; i < zones.Count; i++)
                zones[i].ApplyTo(positions, scales, specs);
        }

        private int StepBudget()
        {
            float factor = 1f;
            IReadOnlyList<SpaceZone> zones = log.Zones;
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].IsActive)
                    factor = Mathf.Max(factor, zones[i].Scale);
            }

            return Mathf.CeilToInt(setup.MaxSteps * factor);
        }
    }
}
