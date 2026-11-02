using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class TrajectoryIntegrator
    {
        public TrajectoryResult Integrate(
            SpacetimeField field, Vector3 start, LaunchSettings launch, TrajectorySettings settings)
        {
            List<Vector3> points = new(settings.MaxSteps + 1);
            StepState state = new(start, launch.Direction * launch.Speed);
            points.Add(state.Position);

            for (int step = 0; step < settings.MaxSteps; step++)
            {
                StepState next = Step(field, settings, state);
                points.Add(next.Position);
                TrajectoryOutcome outcome = Classify(
                    field, state.Position, next.Position, start, settings, out int crashSource);
                state = next;
                if (outcome != TrajectoryOutcome.Miss)
                    return new TrajectoryResult(outcome, points, crashSource);
            }

            return new TrajectoryResult(TrajectoryOutcome.Miss, points);
        }

        private static StepState Step(
            SpacetimeField field, TrajectorySettings settings, StepState state)
        {
            Vector3 acceleration = field.Acceleration(state.Position);
            Vector3 velocity = state.Velocity + acceleration * settings.StepSecs;
            Vector3 position = state.Position
                + velocity * settings.StepSecs / StepDivisor(settings.Zones, state.Position);
            return new StepState(position, velocity);
        }

        private static float StepDivisor(IReadOnlyList<SpaceZone> zones, Vector3 position)
        {
            if (zones == null)
                return 1f;

            float divisor = 1f;
            for (int i = 0; i < zones.Count; i++)
                divisor *= zones[i].StepDivisor(position);

            return divisor;
        }

        private static TrajectoryOutcome Classify(
            SpacetimeField field, Vector3 from, Vector3 to, Vector3 start, TrajectorySettings settings,
            out int crashSource)
        {
            crashSource = NearestCrashSource(field, from, to, start, settings.Destination);
            if (crashSource >= 0)
                return TrajectoryOutcome.Crash;

            if (SegmentHitsSphere(from, to, settings.Destination, settings.CaptureRadius))
                return TrajectoryOutcome.Arrive;

            return TrajectoryOutcome.Miss;
        }

        private static int NearestCrashSource(
            SpacetimeField field, Vector3 from, Vector3 to, Vector3 start, Vector3 destination)
        {
            IReadOnlyList<SpaceSource> sources = field.Sources;
            int crashSource = -1;
            float nearest = float.MaxValue;
            for (int i = 0; i < sources.Count; i++)
            {
                SpaceSource source = sources[i];
                if (IsAt(source.Position, start) || IsAt(source.Position, destination))
                    continue;

                if (!SegmentHit(from, to, source.Position, source.Radius, out float t) || t >= nearest)
                    continue;

                nearest = t;
                crashSource = i;
            }

            return crashSource;
        }

        private static bool IsAt(Vector3 position, Vector3 point) =>
            (position - point).sqrMagnitude < 0.0001f;

        private static bool SegmentHitsSphere(Vector3 from, Vector3 to, Vector3 centre, float radius) =>
            SegmentHit(from, to, centre, radius, out _);

        private static bool SegmentHit(
            Vector3 from, Vector3 to, Vector3 centre, float radius, out float t)
        {
            Vector3 segment = to - from;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.0000001f)
            {
                t = 0f;
                return (from - centre).sqrMagnitude <= radius * radius;
            }

            t = Mathf.Clamp01(Vector3.Dot(centre - from, segment) / lengthSq);
            Vector3 closest = from + segment * t;
            return (closest - centre).sqrMagnitude <= radius * radius;
        }

        private readonly struct StepState
        {
            public readonly Vector3 Position;
            public readonly Vector3 Velocity;

            public StepState(Vector3 position, Vector3 velocity)
            {
                Position = position;
                Velocity = velocity;
            }
        }
    }
}
