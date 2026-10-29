using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class TrajectoryIntegrator
    {
        public TrajectoryResult Integrate(
            SpacetimeField field, Vector3 start, LaunchSettings launch, TrajectorySettings settings)
        {
            List<Vector3> points = new(settings.MaxSteps + 1);
            List<float> times = new(settings.MaxSteps + 1);
            StepState state = new(start, launch.Direction * launch.Speed, 0f);
            points.Add(state.Position);
            times.Add(state.ShipTime);

            for (int step = 0; step < settings.MaxSteps; step++)
            {
                StepState next = Step(field, settings, state);
                points.Add(next.Position);
                times.Add(next.ShipTime);
                TrajectoryOutcome outcome = Classify(
                    field, state.Position, next.Position, start, settings);
                state = next;
                if (outcome != TrajectoryOutcome.Miss)
                    return new TrajectoryResult(outcome, points, times, state.ShipTime);
            }

            return new TrajectoryResult(TrajectoryOutcome.Miss, points, times, state.ShipTime);
        }

        private static StepState Step(
            SpacetimeField field, TrajectorySettings settings, StepState state)
        {
            Vector3 acceleration = field.Acceleration(state.Position);
            float shipTime = state.ShipTime + settings.StepSecs
                * Dilation(field, state.Position, state.Velocity, settings.LightSpeed);
            Vector3 velocity = state.Velocity + acceleration * settings.StepSecs;
            Vector3 position = state.Position
                + velocity * settings.StepSecs / StepDivisor(settings.Zones, state.Position);
            return new StepState(position, velocity, shipTime);
        }

        private static float Dilation(
            SpacetimeField field, Vector3 position, Vector3 velocity, float lightSpeed)
        {
            float c2 = Mathf.Max(lightSpeed * lightSpeed, 0.000001f);
            float depth = 2f * field.Potential(position) / c2;
            float speedTerm = -velocity.sqrMagnitude / c2;
            return Mathf.Sqrt(Mathf.Max(0f, 1f + depth + speedTerm));
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
            SpacetimeField field, Vector3 from, Vector3 to, Vector3 start, TrajectorySettings settings)
        {
            IReadOnlyList<SpaceSource> sources = field.Sources;
            for (int i = 0; i < sources.Count; i++)
            {
                SpaceSource source = sources[i];
                if (IsAt(source.Position, start) || IsAt(source.Position, settings.Destination))
                    continue;

                if (SegmentHitsSphere(from, to, source.Position, source.Radius))
                    return TrajectoryOutcome.Crash;
            }

            if (SegmentHitsSphere(from, to, settings.Destination, settings.CaptureRadius))
                return TrajectoryOutcome.Arrive;

            return TrajectoryOutcome.Miss;
        }

        private static bool IsAt(Vector3 position, Vector3 point) =>
            (position - point).sqrMagnitude < 0.0001f;

        private static bool SegmentHitsSphere(Vector3 from, Vector3 to, Vector3 centre, float radius)
        {
            Vector3 segment = to - from;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.0000001f)
                return (from - centre).sqrMagnitude <= radius * radius;

            float t = Mathf.Clamp01(Vector3.Dot(centre - from, segment) / lengthSq);
            Vector3 closest = from + segment * t;
            return (closest - centre).sqrMagnitude <= radius * radius;
        }

        private readonly struct StepState
        {
            public readonly Vector3 Position;
            public readonly Vector3 Velocity;
            public readonly float ShipTime;

            public StepState(Vector3 position, Vector3 velocity, float shipTime)
            {
                Position = position;
                Velocity = velocity;
                ShipTime = shipTime;
            }
        }
    }
}
