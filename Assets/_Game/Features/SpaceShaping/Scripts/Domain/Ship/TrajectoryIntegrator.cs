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
            Vector3 position = start;
            Vector3 velocity = launch.Direction * launch.Speed;
            float shipTime = 0f;
            points.Add(position);
            times.Add(shipTime);

            for (int step = 0; step < settings.MaxSteps; step++)
            {
                Vector3 acceleration = field.Acceleration(position);
                shipTime += settings.StepSecs * Dilation(field, position, velocity, settings.LightSpeed);
                velocity += acceleration * settings.StepSecs;
                Vector3 from = position;
                position += velocity * settings.StepSecs / StepDivisor(settings.Zones, position);
                points.Add(position);
                times.Add(shipTime);

                TrajectoryOutcome outcome = Classify(field, from, position, start, settings);
                if (outcome != TrajectoryOutcome.Miss)
                    return new TrajectoryResult(outcome, points, times, shipTime);
            }

            return new TrajectoryResult(TrajectoryOutcome.Miss, points, times, shipTime);
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
    }
}
