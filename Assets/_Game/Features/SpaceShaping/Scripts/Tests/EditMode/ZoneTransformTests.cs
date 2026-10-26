using System.Collections.Generic;
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

        private static TrajectorySettings Settings(IReadOnlyList<SpaceZone> zones = null, int maxSteps = 4000) => new()
        {
            Destination = new Vector3(4f, 0f, 0f),
            CaptureRadius = 0.6f,
            StepSecs = 1f / 120f,
            MaxSteps = maxSteps,
            DilationScale = 1f,
            Zones = zones
        };

        private static SpaceZone[] Zones(params SpaceZone[] zones) => zones;

        private static SpaceZone Covering(float scale) =>
            new(Vector3.zero, new Vector3(4f, 4f, 4f), scale, 0f);

        [Test]
        public void InactiveZone_MatchesNoZone()
        {
            TrajectoryIntegrator integrator = new();
            SpacetimeField field = new();
            Vector3 origin = new(-4f, 0f, 0f);

            TrajectoryResult plain = integrator.Integrate(field, origin, Launch(), Settings());
            TrajectoryResult withZone = integrator.Integrate(
                field, origin, Launch(), Settings(Zones(new SpaceZone(Vector3.zero, Vector3.zero, 2f, 0f))));

            Assert.AreEqual(plain.Points.Count, withZone.Points.Count);
            Assert.AreEqual(plain.ShipTime, withZone.ShipTime, 0.0001f);
        }

        [Test]
        public void MagnifiedZone_LengthensShipTime()
        {
            TrajectoryIntegrator integrator = new();
            SpacetimeField field = new();
            Vector3 origin = new(-4f, 0f, 0f);

            TrajectoryResult plain = integrator.Integrate(field, origin, Launch(), Settings());
            TrajectoryResult magnified = integrator.Integrate(field, origin, Launch(), Settings(Zones(Covering(2f))));

            Assert.AreEqual(TrajectoryOutcome.Arrive, magnified.Outcome);
            Assert.Greater(magnified.ShipTime, plain.ShipTime * 1.5f);
        }

        [Test]
        public void ShrunkZone_ShortensShipTime()
        {
            TrajectoryIntegrator integrator = new();
            SpacetimeField field = new();
            Vector3 origin = new(-4f, 0f, 0f);

            TrajectoryResult plain = integrator.Integrate(field, origin, Launch(), Settings());
            TrajectoryResult shrunk = integrator.Integrate(field, origin, Launch(), Settings(Zones(Covering(0.5f))));

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

            TrajectoryResult first = integrator.Integrate(field, origin, Launch(), Settings(Zones(zone)));
            TrajectoryResult second = integrator.Integrate(field, origin, Launch(), Settings(Zones(zone)));

            Assert.AreEqual(first.Points.Count, second.Points.Count);
            for (int i = 0; i < first.Points.Count; i++)
                Assert.AreEqual(first.Points[i], second.Points[i]);
        }

        [Test]
        public void Session_AppliesSetupZone()
        {
            SpaceShapingSession session = new(SessionSetup(4000));
            session.ApplyZone(Covering(2f));
            TrajectoryResult result = session.Simulate(session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

            Assert.Greater(result.ShipTime, 10f);
        }

        [Test]
        public void MagnifiedZone_RaisesStepBudget()
        {
            SpaceShapingSession session = new(SessionSetup(950));
            session.ApplyZone(Covering(2f));
            TrajectoryResult result = session.Simulate(session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

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

        [Test]
        public void Accumulate_AppliesZonesInOrder()
        {
            SpaceShapingSession session = new(ContentSetup());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 0.5f, 0f));

            Assert.AreEqual(-2f, session.GetPosition(0).x, 0.0001f);
            Assert.AreEqual(2f, session.GetScale(0), 0.0001f);
            Assert.AreEqual(2, session.CommandCount);
            Assert.AreEqual(2, session.Zones.Count);
        }

        [Test]
        public void Undo_RemovesLastCommandAndRestoresContent()
        {
            SpaceShapingSession session = new(ContentSetup());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));
            Assert.AreEqual(4f, session.GetScale(0), 0.0001f);

            Assert.IsTrue(session.Undo());
            Assert.AreEqual(2f, session.GetScale(0), 0.0001f);
            Assert.AreEqual(0, session.CommandCount);
            Assert.IsFalse(session.Undo());
        }

        [Test]
        public void Gesture_AccumulatesEntropySpentPerFrame()
        {
            SpaceShapingSession session = new(ContentSetup());
            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1f, 0f));
            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1.5f, 0f));
            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));

            Assert.AreEqual(1f, session.EntropySpent, 0.0001f);

            session.CommitPreview();
            Assert.AreEqual(1, session.CommandCount);
            Assert.AreEqual(1f, session.EntropySpent, 0.0001f);
        }

        [Test]
        public void Undo_RefundsEntropySpent()
        {
            SpaceShapingSession session = new(ContentSetup());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));
            Assert.AreEqual(1f, session.EntropySpent, 0.0001f);

            session.Undo();
            Assert.AreEqual(0f, session.EntropySpent, 0.0001f);
        }

        private static SpaceShapingSetup SessionSetup(int maxSteps) => new()
        {
            Origin = new Vector3(-4f, 0f, 0f),
            Destination = new Vector3(4f, 0f, 0f),
            CaptureRadius = 0.6f,
            StepSecs = 1f / 120f,
            MaxSteps = maxSteps,
            LaunchModel = LaunchModel.Coast,
            LaunchSpeed = 1f,
            ThrustAcceleration = 1f,
            SourceStrengthScale = 2f,
            SeparationScale = 1.2f,
            DilationScale = 1f,
            Sources = System.Array.Empty<SourceSpec>()
        };

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
