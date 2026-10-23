using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class SpaceShapingSolveTests
    {
        private static SpaceShapingSetup Setup(float deadline) => new()
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
            Deadline = deadline,
            Sources = System.Array.Empty<SourceSpec>()
        };

        private static TrajectoryResult Simulate(SpaceShapingSession session) =>
            session.Simulate(session.BuildField(), new TrajectoryIntegrator(), new ShipLaunch());

        [Test]
        public void Arrival_InsideDeadline_IsSolved()
        {
            SpaceShapingSession session = new(Setup(9f));
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Arrive, result.Outcome);
            Assert.Greater(result.ShipTime, 7f);
            Assert.Less(result.ShipTime, 9f);
            Assert.IsTrue(session.IsSolved(result));
        }

        [Test]
        public void Arrival_OutsideDeadline_IsNotSolved()
        {
            SpaceShapingSession session = new(Setup(7f));
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Arrive, result.Outcome);
            Assert.IsFalse(session.IsSolved(result));
        }

        [Test]
        public void Miss_IsNotSolved()
        {
            SpaceShapingSetup setup = Setup(9f);
            setup.Destination = new Vector3(400f, 0f, 0f);
            SpaceShapingSession session = new(setup);
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Miss, result.Outcome);
            Assert.IsFalse(session.IsSolved(result));
        }
    }
}
