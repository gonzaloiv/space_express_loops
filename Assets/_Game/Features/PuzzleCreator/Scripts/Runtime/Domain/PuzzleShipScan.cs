using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The inverted search's primitive: given one candidate zone, which runs does it rescue? A run is an
    // ordered planet pair that misses (or crashes) on the pristine field and arrives once the zone is
    // applied, with clearance. The ships of a multi-ship puzzle are the rescued runs of the edited field.
    //
    // A pair's pristine outcome does not depend on the zone, so it is computed once per field; a sweep
    // can then test many zones against the same field cheaply.
    public sealed class PuzzleShipScan
    {
        private readonly SpaceShapingSetup setup;
        private readonly PuzzleData puzzle;
        private readonly PuzzleSimSettings settings;
        private readonly float clearance;
        private readonly ShipLaunch launch = new();
        private readonly TrajectoryIntegrator integrator = new();
        private readonly FieldView start;
        private readonly bool[] startArrives;
        private readonly int count;

        public PuzzleShipScan(PuzzleData puzzle, PuzzleSimSettings settings, float clearance)
        {
            this.puzzle = puzzle;
            this.settings = settings;
            this.clearance = clearance;
            count = puzzle != null && puzzle.sources != null ? puzzle.sources.Length : 0;
            startArrives = new bool[count * count];
            if (count < 2)
                return;

            setup = SpaceShapingSetup.Build(puzzle, settings);
            launch.LaunchSpeed = setup.LaunchSpeed;
            start = FieldView.Build(setup, null);
            CacheStart();
        }

        // Runtime semantics: a zone buys time (its scale inflates the step budget).
        public List<PuzzleShipRescue> Rescued(PuzzleZone zone) => Run(zone, true);

        // Field-only: the step budget is held fixed, so only the edit itself can rescue a run.
        public List<PuzzleShipRescue> RescuedFixed(PuzzleZone zone) => Run(zone, false);

        // Independent re-check of a chosen ship set: every ship misses on the pristine field and arrives
        // with the zone applied. The bake uses this to confirm what the rescue scan found.
        public bool Rescues(PuzzleZone zone, IReadOnlyList<PuzzleShip> ships)
        {
            if (count < 2 || ships == null || ships.Count == 0)
                return false;

            FieldView edited = FieldView.Build(setup, zone);
            for (int i = 0; i < ships.Count; i++)
            {
                if (!Rescues(edited, ships[i]))
                    return false;
            }

            return true;
        }

        private bool Rescues(FieldView edited, PuzzleShip ship)
        {
            float capture = ship.captureRadius;
            if (capture <= 0f)
                return false;

            if (Run(start, ship.origin, ship.destination, capture, false).Outcome
                == TrajectoryOutcome.Arrive)
                return false;

            return Run(edited, ship.origin, ship.destination, capture, true).Outcome
                == TrajectoryOutcome.Arrive;
        }

        private void CacheStart()
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i == j)
                        continue;

                    float capture = puzzle.sources[j].scale * 0.5f;
                    if (capture <= 0f)
                        continue;

                    startArrives[i * count + j] =
                        Run(start, i, j, capture, false).Outcome == TrajectoryOutcome.Arrive;
                }
            }
        }

        private List<PuzzleShipRescue> Run(PuzzleZone zone, bool timeDilation)
        {
            List<PuzzleShipRescue> rescued = new();
            if (count < 2)
                return rescued;

            FieldView edited = FieldView.Build(setup, zone);
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                    Consider(edited, zone, i, j, timeDilation, rescued);
            }

            return rescued;
        }

        private void Consider(
            FieldView edited, PuzzleZone zone, int i, int j, bool timeDilation,
            List<PuzzleShipRescue> rescued)
        {
            if (i == j || startArrives[i * count + j])
                return;

            Vector3 origin = puzzle.sources[i].home;
            Vector3 destination = puzzle.sources[j].home;
            float capture = puzzle.sources[j].scale * 0.5f;
            if (capture <= 0f || zone.Influences(origin) || zone.Influences(destination))
                return;

            TrajectoryResult result = Run(edited, i, j, capture, timeDilation);
            if (result.Outcome != TrajectoryOutcome.Arrive)
                return;

            float margin = capture - result.ClosestApproach(destination);
            if (margin >= clearance)
                rescued.Add(new PuzzleShipRescue(origin, destination, margin));
        }

        private TrajectoryResult Run(FieldView view, int i, int j, float capture, bool timeDilation) =>
            Run(view, puzzle.sources[i].home, puzzle.sources[j].home, capture, timeDilation);

        private TrajectoryResult Run(
            FieldView view, Vector3 origin, Vector3 destination, float capture, bool timeDilation)
        {
            TrajectorySettings trajectory = new()
            {
                Destination = destination,
                CaptureRadius = capture,
                StepSecs = settings.StepSecs,
                MaxSteps = Budget(view.Zones, origin, destination, timeDilation),
                Zones = view.Zones
            };

            return launch.Simulate(view.Field, origin, destination, trajectory, integrator);
        }

        private int Budget(
            IReadOnlyList<SpaceZone> zones, Vector3 origin, Vector3 destination, bool timeDilation)
        {
            if (settings.NominalSpeed <= 0f || settings.StepSecs <= 0f)
                return 1;

            float factor = 1f;
            if (timeDilation && zones != null)
            {
                for (int i = 0; i < zones.Count; i++)
                {
                    if (zones[i].IsActive)
                        factor = Mathf.Max(factor, zones[i].Scale);
                }
            }

            float run = Vector3.Distance(origin, destination);
            return Mathf.Max(
                1, Mathf.CeilToInt(run / settings.NominalSpeed / settings.StepSecs * factor));
        }

        private readonly struct FieldView
        {
            public readonly SpacetimeField Field;
            public readonly IReadOnlyList<SpaceZone> Zones;

            private FieldView(SpacetimeField field, IReadOnlyList<SpaceZone> zones)
            {
                Field = field;
                Zones = zones;
            }

            public static FieldView Build(SpaceShapingSetup setup, PuzzleZone? zone)
            {
                SpaceShapingSession session = new(setup);
                if (zone.HasValue)
                    session.ApplyZone(zone.Value.ToSpaceZone());

                return new FieldView(session.BuildField(), session.Zones);
            }
        }
    }
}
