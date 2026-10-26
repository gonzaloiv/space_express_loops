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
            Vector3 position = start;
            Vector3 velocity = launch.Direction * launch.Speed;
            float shipTime = 0f;
            points.Add(position);

            for (int step = 0; step < settings.MaxSteps; step++)
            {
                Vector3 acceleration = field.Acceleration(position);
                shipTime += settings.StepSecs * Dilation(field, position, settings.DilationScale);
                velocity += acceleration * settings.StepSecs;
                Vector3 from = position;
                position += velocity * settings.StepSecs / StepDivisor(settings.Zones, position);
                points.Add(position);

                TrajectoryOutcome outcome = Classify(field, from, position, start, settings);
                if (outcome != TrajectoryOutcome.Miss)
                    return new TrajectoryResult(outcome, points, shipTime);
            }

            return new TrajectoryResult(TrajectoryOutcome.Miss, points, shipTime);
        }

        private static float Dilation(SpacetimeField field, Vector3 position, float scale) =>
            Mathf.Clamp(1f + scale * field.Potential(position), 0.05f, 1f);

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
                bool isOrigin = (source.Position - start).sqrMagnitude < 0.0001f;
                if (!isOrigin && SegmentHitsSphere(from, to, source.Position, source.Radius))
                    return TrajectoryOutcome.Crash;
            }

            if (SegmentHitsSphere(from, to, settings.Destination, settings.CaptureRadius))
                return TrajectoryOutcome.Arrive;

            return TrajectoryOutcome.Miss;
        }

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
