using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class TrajectoryDeterminismTests
    {
        private static LaunchSettings Launch() => new()
        {
            Direction = Vector3.right,
            Speed = 1f
        };

        private static TrajectorySettings Settings() => new()
        {
            Destination = new Vector3(4f, 0f, 0f),
            CaptureRadius = 0.6f,
            StepSecs = 1f / 120f,
            MaxSteps = 1200,
            LightSpeed = 3.1622777f
        };

        private static SpacetimeField Field()
        {
            SpacetimeField field = new();
            field.Add(new SpaceSource(new Vector3(-1f, 1.2f, 0.8f), 1.2f, 2.4f));
            field.Add(new SpaceSource(new Vector3(1.5f, -1.2f, -0.8f), 1.2f, 2.4f));
            return field;
        }

        [Test]
        public void SameInputs_ProduceIdenticalPath()
        {
            SpacetimeField field = Field();
            TrajectoryIntegrator integrator = new();
            Vector3 origin = new(-4f, 0f, 0f);

            TrajectoryResult first = integrator.Integrate(field, origin, Launch(), Settings());
            TrajectoryResult second = integrator.Integrate(field, origin, Launch(), Settings());

            Assert.AreEqual(first.Outcome, second.Outcome);
            Assert.AreEqual(first.Points.Count, second.Points.Count);
            for (int i = 0; i < first.Points.Count; i++)
                Assert.AreEqual(first.Points[i], second.Points[i]);
        }

        [Test]
        public void EmptyField_IsStraightLine()
        {
            TrajectoryIntegrator integrator = new();
            TrajectoryResult result = integrator.Integrate(new SpacetimeField(), new Vector3(-4f, 0f, 0f), Launch(), Settings());

            Assert.Greater(result.Points.Count, 1);
            for (int i = 0; i < result.Points.Count; i++)
            {
                Assert.AreEqual(0f, result.Points[i].y, 0.0001f);
                Assert.AreEqual(0f, result.Points[i].z, 0.0001f);
            }
        }

        [Test]
        public void Clock_SamplesPerPointShipTime()
        {
            TrajectoryResult result = new TrajectoryIntegrator().Integrate(
                new SpacetimeField(), new Vector3(-4f, 0f, 0f), Launch(), Settings());

            Assert.AreEqual(result.Points.Count, result.Times.Count);
            Assert.AreEqual(0f, result.Times[0], 0.00001f);
            Assert.AreEqual(result.ShipTime, result.Times[result.Times.Count - 1], 0.00001f);
            for (int i = 1; i < result.Times.Count; i++)
                Assert.GreaterOrEqual(result.Times[i], result.Times[i - 1]);

            Assert.AreEqual(0f, result.SampleTime(0f), 0.00001f);
            Assert.AreEqual(result.ShipTime, result.SampleTime(result.Points.Count - 1), 0.00001f);
        }

        [Test]
        public void SessionPath_MatchesDirectRun()
        {
            SpaceShapingSetup setup = new()
            {
                Origin = new Vector3(-4f, 0f, 0f),
                Destination = new Vector3(4f, 0f, 0f),
                MaxSteps = 1200,
                LaunchSpeed = 1f,
                Sim = new PuzzleSimSettings
                {
                    CaptureRadius = 0.6f,
                    StepSecs = 1f / 120f,
                    SourceStrengthScale = 2f,
                    SeparationScale = 1.2f,
                    LightSpeed = 3.1622777f
                },
                Sources = new[]
                {
                    new SourceSpec { Home = new Vector3(-1f, 1.2f, 0.8f), InitialScale = 2.4f, MinScale = 1.2f, MaxScale = 3.5f },
                    new SourceSpec { Home = new Vector3(1.5f, -1.2f, -0.8f), InitialScale = 2.4f, MinScale = 1.2f, MaxScale = 3.5f }
                }
            };

            SpaceShapingSession session = new(setup);
            SpacetimeField field = session.BuildField();
            TrajectoryResult viaSession = session.Simulate(field, new TrajectoryIntegrator(), new ShipLaunch());
            TrajectoryResult direct = new TrajectoryIntegrator().Integrate(field, setup.Origin, Launch(), Settings());

            Assert.AreEqual(direct.Outcome, viaSession.Outcome);
            Assert.AreEqual(direct.Points.Count, viaSession.Points.Count);
            for (int i = 0; i < direct.Points.Count; i++)
                Assert.AreEqual(direct.Points[i], viaSession.Points[i]);
        }
    }
}
