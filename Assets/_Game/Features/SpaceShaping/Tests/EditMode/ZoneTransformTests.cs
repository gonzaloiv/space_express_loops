using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class ZoneTransformTests
    {
        private static LaunchSettings Launch() => new()
        {
            Model = LaunchModel.Coast,
            Direction = Vector3.right,
            Speed = 1f
        };

        private static TrajectorySettings Settings(SpaceZone zone, int maxSteps = 4000) => new()
        {
            Destination = new Vector3(4f, 0f, 0f),
            CaptureRadius = 0.6f,
            StepSecs = 1f / 120f,
            MaxSteps = maxSteps,
            DilationScale = 1f,
            Zone = zone
        };

        private static SpaceZone Covering(float scale) =>
            new(Vector3.zero, new Vector3(4f, 4f, 4f), scale, 0f);

        [Test]
        public void InactiveZone_MatchesNoZone()
        {
            TrajectoryIntegrator integrator = new();
            SpacetimeField field = new();
            Vector3 origin = new(-4f, 0f, 0f);

            TrajectoryResult plain = integrator.Integrate(field, origin, Launch(), Settings(default));
            TrajectoryResult withZone = integrator.Integrate(
                field, origin, Launch(), Settings(new SpaceZone(Vector3.zero, Vector3.zero, 2f, 0f)));

            Assert.AreEqual(plain.Points.Count, withZone.Points.Count);
            Assert.AreEqual(plain.ShipTime, withZone.ShipTime, 0.0001f);
        }

        [Test]
        public void MagnifiedZone_LengthensShipTime()
        {
            TrajectoryIntegrator integrator = new();
            SpacetimeField field = new();
            Vector3 origin = new(-4f, 0f, 0f);

            TrajectoryResult plain = integrator.Integrate(field, origin, Launch(), Settings(default));
            TrajectoryResult magnified = integrator.Integrate(field, origin, Launch(), Settings(Covering(2f)));

            Assert.AreEqual(TrajectoryOutcome.Arrive, magnified.Outcome);
            Assert.Greater(magnified.ShipTime, plain.ShipTime * 1.5f);
        }

        [Test]
        public void ShrunkZone_ShortensShipTime()
        {
            TrajectoryIntegrator integrator = new();
            SpacetimeField field = new();
            Vector3 origin = new(-4f, 0f, 0f);

            TrajectoryResult plain = integrator.Integrate(field, origin, Launch(), Settings(default));
            TrajectoryResult shrunk = integrator.Integrate(field, origin, Launch(), Settings(Covering(0.5f)));

            Assert.AreEqual(TrajectoryOutcome.Arrive, shrunk.Outcome);
            Assert.Less(shrunk.ShipTime, plain.ShipTime * 0.75f);
        }

        [Test]
        public void LargeStep_DoesNotTunnelThroughPlanet()
        {
            TrajectoryIntegrator integrator = new();
            SpacetimeField field = new();
            field.Add(new SpaceSource(Vector3.zero, 0.3f, 2f));

            TrajectorySettings settings = new()
            {
                Destination = new Vector3(40f, 0f, 0f),
                CaptureRadius = 0.6f,
                StepSecs = 1f,
                MaxSteps = 20,
                DilationScale = 1f
            };

            TrajectoryResult result = integrator.Integrate(field, new Vector3(-4.5f, 0f, 0f), Launch(), settings);

            Assert.AreEqual(TrajectoryOutcome.Crash, result.Outcome);
        }

        [Test]
        public void Zone_IsDeterministic()
        {
            TrajectoryIntegrator integrator = new();
            SpacetimeField field = new();
            Vector3 origin = new(-4f, 0f, 0f);
            SpaceZone zone = new(new Vector3(1f, 0f, 0f), new Vector3(2f, 2f, 2f), 1.5f, 0.5f);

            TrajectoryResult first = integrator.Integrate(field, origin, Launch(), Settings(zone));
            TrajectoryResult second = integrator.Integrate(field, origin, Launch(), Settings(zone));

            Assert.AreEqual(first.Points.Count, second.Points.Count);
            for (int i = 0; i < first.Points.Count; i++)
                Assert.AreEqual(first.Points[i], second.Points[i]);
        }

        [Test]
        public void Session_AppliesSetupZone()
        {
            SpaceShapingSetup setup = new()
            {
                Origin = new Vector3(-4f, 0f, 0f),
                Destination = new Vector3(4f, 0f, 0f),
                CaptureRadius = 0.6f,
                StepSecs = 1f / 120f,
                MaxSteps = 4000,
                LaunchModel = LaunchModel.Coast,
                LaunchSpeed = 1f,
                ThrustAcceleration = 1f,
                SourceStrengthScale = 2f,
                SeparationScale = 1.2f,
                DilationScale = 1f,
                Sources = System.Array.Empty<SourceSpec>()
            };

            SpaceShapingSession session = new(setup);
            TrajectoryResult result = session.Simulate(
                session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch(), Covering(2f));

            Assert.Greater(result.ShipTime, 10f);
        }
        [Test]
        public void MagnifiedZone_RaisesStepBudget()
        {
            SpaceShapingSetup setup = new()
            {
                Origin = new Vector3(-4f, 0f, 0f),
                Destination = new Vector3(4f, 0f, 0f),
                CaptureRadius = 0.6f,
                StepSecs = 1f / 120f,
                MaxSteps = 950,
                LaunchModel = LaunchModel.Coast,
                LaunchSpeed = 1f,
                ThrustAcceleration = 1f,
                SourceStrengthScale = 2f,
                SeparationScale = 1.2f,
                DilationScale = 1f,
                Sources = System.Array.Empty<SourceSpec>()
            };

            SpaceShapingSession session = new(setup);
            TrajectoryResult result = session.Simulate(
                session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch(), Covering(2f));

            Assert.AreEqual(TrajectoryOutcome.Arrive, result.Outcome);
        }
        [Test]
        public void Session_ZoneSpreadsAndGrowsContent()
        {
            SpaceShapingSession session = new(ContentSetup());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));

            Assert.AreEqual(new Vector3(-4f, 0f, 0f), session.GetPosition(0));
            Assert.AreEqual(4f, session.GetScale(0), 0.0001f);
        }

        [Test]
        public void Session_OutsideZone_IsUnchanged()
        {
            SpaceShapingSession session = new(ContentSetup());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(1f, 1f, 1f), 2f, 0f));

            Assert.AreEqual(new Vector3(-2f, 0f, 0f), session.GetPosition(0));
            Assert.AreEqual(2f, session.GetScale(0), 0.0001f);
        }

        private static SpaceShapingSetup ContentSetup() => new()
        {
            Origin = new Vector3(-4f, 0f, 0f),
            Destination = new Vector3(4f, 0f, 0f),
            CaptureRadius = 0.6f,
            StepSecs = 1f / 120f,
            MaxSteps = 4000,
            LaunchModel = LaunchModel.Coast,
            LaunchSpeed = 1f,
            ThrustAcceleration = 1f,
            SourceStrengthScale = 2f,
            SeparationScale = 1.2f,
            DilationScale = 1f,
            Sources = new[]
            {
                new SourceSpec { Home = new Vector3(-2f, 0f, 0f), InitialScale = 2f, MoveRange = 2f, MinScale = 1f, MaxScale = 4f }
            }
        };
    }
}
