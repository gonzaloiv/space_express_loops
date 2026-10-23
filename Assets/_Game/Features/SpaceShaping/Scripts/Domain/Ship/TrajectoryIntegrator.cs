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
            Vector3 velocity = launch.Model == LaunchModel.Coast ? launch.Direction * launch.Speed : Vector3.zero;
            float shipTime = 0f;
            points.Add(position);

            for (int step = 0; step < settings.MaxSteps; step++)
            {
                Vector3 acceleration = field.Acceleration(position);
                if (launch.Model == LaunchModel.Thrust)
                    acceleration += launch.Direction * launch.ThrustAcceleration;

                shipTime += settings.StepSecs * Dilation(field, position, settings.DilationScale);
                velocity += acceleration * settings.StepSecs;
                position += velocity * settings.StepSecs;
                points.Add(position);

                TrajectoryOutcome outcome = Classify(field, position, start, settings);
                if (outcome != TrajectoryOutcome.Miss)
                    return new TrajectoryResult(outcome, points, shipTime);
            }

            return new TrajectoryResult(TrajectoryOutcome.Miss, points, shipTime);
        }

        private static float Dilation(SpacetimeField field, Vector3 position, float scale) =>
            Mathf.Clamp(1f + scale * field.Potential(position), 0.05f, 1f);

        private static TrajectoryOutcome Classify(
            SpacetimeField field, Vector3 position, Vector3 start, TrajectorySettings settings)
        {
            IReadOnlyList<SpaceSource> sources = field.Sources;
            for (int i = 0; i < sources.Count; i++)
            {
                SpaceSource source = sources[i];
                bool isOrigin = (source.Position - start).sqrMagnitude < 0.0001f;
                if (!isOrigin && (position - source.Position).magnitude <= source.Radius)
                    return TrajectoryOutcome.Crash;
            }

            if ((position - settings.Destination).magnitude <= settings.CaptureRadius)
                return TrajectoryOutcome.Arrive;

            return TrajectoryOutcome.Miss;
        }
    }
}
