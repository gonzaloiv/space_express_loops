using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class SpaceShapingSolveTests
    {
        private static SpaceShapingSetup Setup(float tmin, float tmax) => new()
        {
            Origin = new Vector3(-4f, 0f, 0f),
            Destination = new Vector3(4f, 0f, 0f),
            CaptureRadius = 0.6f,
            StepSecs = 1f / 120f,
            MaxSteps = 1200,
            LaunchModel = LaunchModel.Coast,
            LaunchSpeed = 1f,
            ThrustAcceleration = 1f,
            SourceStrengthScale = 2f,
            SeparationScale = 1.2f,
            DilationScale = 1f,
            Tmin = tmin,
            Tmax = tmax,
            EntropyBudget = 1f,
            EntropyPar = 0.5f,
            Sources = System.Array.Empty<SourceSpec>()
        };

        private static TrajectoryResult Simulate(SpaceShapingSession session) =>
            session.Simulate(session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

        [Test]
        public void Arrival_InsideWindow_IsSolved()
        {
            SpaceShapingSession session = new(Setup(0f, 9f));
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Arrive, result.Outcome);
            Assert.Greater(result.ShipTime, 7f);
            Assert.Less(result.ShipTime, 9f);
            Assert.IsTrue(session.IsSolved(result));
        }

        [Test]
        public void Arrival_AboveWindow_IsNotSolved()
        {
            SpaceShapingSession session = new(Setup(0f, 7f));
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Arrive, result.Outcome);
            Assert.IsFalse(session.IsSolved(result));
        }

        [Test]
        public void Arrival_BelowWindow_IsNotSolved()
        {
            SpaceShapingSession session = new(Setup(8f, 9f));
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Arrive, result.Outcome);
            Assert.Less(result.ShipTime, 8f);
            Assert.IsFalse(session.IsSolved(result));
        }

        [Test]
        public void Miss_IsNotSolved()
        {
            SpaceShapingSetup setup = Setup(0f, 9f);
            setup.Destination = new Vector3(400f, 0f, 0f);
            SpaceShapingSession session = new(setup);
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Miss, result.Outcome);
            Assert.IsFalse(session.IsSolved(result));
        }

        [Test]
        public void Grade_BucketsEntropyLeft()
        {
            SpaceShapingSession session = new(Setup(0f, 9f));

            Assert.AreEqual(3, session.Grade(0.5f));
            Assert.AreEqual(2, session.Grade(0.1f));
            Assert.AreEqual(1, session.Grade(-0.1f));
        }
    }
}
