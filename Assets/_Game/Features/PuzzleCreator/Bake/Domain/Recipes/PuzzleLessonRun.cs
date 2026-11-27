using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleLessonRun
    {
        private const float OpenBudget = 1e9f;
        private const float MinDeviationFraction = 0.05f;
        private const float MinAngle = 4f;

        public static SpaceShapingSetup Setup(
            PuzzleFieldPlanet[] planets, PuzzleLessonContext context) =>
            SpaceShapingSetup.Build(new SpaceShapingInputs
            {
                Origin = context.Origin,
                Destination = context.Destination,
                Sources = SourceSpecs.From(Sources(planets, context), context.Settings),
                Ships = ShipSpecList(context),
                CaptureRadius = Capture(context),
                EntropyBudget = OpenBudget,
                NoFail = context.Brief.NoFail,
                NoGrade = context.Brief.NoGrade,
                Settings = context.Settings
            });

        public static bool Arrives(SpaceShapingSetup setup, PuzzleZone zone)
        {
            SpaceShapingSession session = new(setup);
            session.ApplyZone(zone.ToSpaceZone());
            return session.SimulateAll().AllArrived();
        }

        public static TrajectoryResult[] StartResults(SpaceShapingSetup setup) =>
            new SpaceShapingSession(setup).SimulateAll();

        public static bool StartsAsTheBriefAsks(
            SpaceShapingSetup setup, PuzzleLessonContext context, out TrajectoryResult start)
        {
            TrajectoryResult[] results = StartResults(setup);
            start = results.Length > 0 ? results[0] : default;
            return StartsAsTheBriefAsks(results, context);
        }

        public static bool StartsAsTheBriefAsks(
            TrajectoryResult[] results, PuzzleLessonContext context)
        {
            if (results.Length == 0 || results.AllArrived())
                return false;

            TrajectoryResult start = results[0];
            if (context.Brief.NoFail && start.Outcome != TrajectoryOutcome.Crash)
                return false;

            return !NeedsCurve(context.Brief.Mechanism) || Curves(start, context);
        }

        // A fleet's other paths are held by their blockers; only the shared bend counts as the start's
        // cause, so every ship must miss at the start, not just one.
        public static bool StartsFailForEveryShip(TrajectoryResult[] results)
        {
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].Outcome == TrajectoryOutcome.Arrive)
                    return false;
            }

            return true;
        }

        private static bool Curves(TrajectoryResult start, PuzzleLessonContext context)
        {
            float run = Vector3.Distance(context.Origin, context.Destination);
            if (run <= 0f)
                return false;
            float deviation = PuzzleRunGeometry.MaxChordDeviation(
                start, context.Origin, context.Destination);
            if (deviation / run < MinDeviationFraction)
                return false;
            return PuzzleRunGeometry.MaxAngle(start, context.Origin, context.Destination)
                >= MinAngle;
        }

        private static bool NeedsCurve(PuzzleMechanism mechanism) =>
            mechanism == PuzzleMechanism.Curve || mechanism == PuzzleMechanism.Mixed;

        private static float Capture(PuzzleLessonContext context) =>
            context.CaptureRadius > 0f
                ? context.CaptureRadius
                : context.Planets[context.DestinationIndex].radius;

        private static ShipSpec[] ShipSpecList(PuzzleLessonContext context)
        {
            if (context.Ships == null || context.Ships.Length == 0)
                return ShipSpecs.From(null, context.Origin, context.Destination);

            ShipSpec[] specs = new ShipSpec[context.Ships.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                PuzzleConstructedShip ship = context.Ships[i];
                specs[i] = new ShipSpec
                {
                    Origin = ship.Origin,
                    Destination = ship.Destination,
                    CaptureRadius = ship.CaptureRadius
                };
            }

            return specs;
        }

        private static PuzzleSource[] Sources(
            PuzzleFieldPlanet[] planets, PuzzleLessonContext context)
        {
            PuzzleSource[] sources = PuzzleSourceBridge.ToSources(planets);
            if (context.Brief.SunRadius <= 0f
                || context.Brief.SunPlacement != PuzzleSunPlacement.Centre)
                return sources;

            PuzzleSource[] withSun = new PuzzleSource[sources.Length + 1];
            withSun[0] = new PuzzleSource
            {
                home = Vector3.zero,
                scale = context.Brief.SunRadius * 2f,
                fixedMass = !context.Brief.SunScalable,
                centreOnly = true
            };
            for (int i = 0; i < sources.Length; i++)
                withSun[i + 1] = sources[i];
            return withSun;
        }
    }
}
