using DigitalLove.Game.SpaceShaping.Cube;
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
            MaxSteps = 1200,
            LaunchSpeed = 1f,
            Tmin = tmin,
            Tmax = tmax,
            EntropyBudget = 1f,
            Sim = new PuzzleSimSettings
            {
                CaptureRadius = 0.6f,
                StepSecs = 1f / 120f,
                SourceStrengthScale = 2f,
                SeparationScale = 1.2f,
                LightSpeed = 3.1622777f
            },
            Sources = System.Array.Empty<SourceSpec>()
        };

        private static TrajectoryResult Simulate(SpaceShapingSession session) =>
            session.Simulate();

        [Test]
        public void Clock_MatchesTheTwoTermForm()
        {
            SpaceShapingSession session = new(Setup(0f, 9f));
            TrajectoryResult result = Simulate(session);

            float c2 = 3.1622777f * 3.1622777f;
            float travel = 8f - 0.6f;
            float expected = travel * Mathf.Sqrt(1f - 1f / c2);
            Assert.AreEqual(expected, result.ShipTime, 0.02f);
        }

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
        public void Crash_IsNotSolved()
        {
            SpaceShapingSetup setup = Setup(0f, 9f);
            setup.Sources = new[]
            {
                new SourceSpec { Home = Vector3.zero, InitialScale = 4f, MinScale = 1f, MaxScale = 4f }
            };
            SpaceShapingSession session = new(setup);
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Crash, result.Outcome);
            Assert.IsFalse(session.IsSolved(result));
        }

        [Test]
        public void Entropy_IsChargedPerModification_NotPerSimulation()
        {
            SpaceShapingSession session = new(Setup(0f, 9f));
            session.ApplyZone(Zone(1.5f));
            float spent = session.EntropySpent;

            Simulate(session);
            Simulate(session);
            Simulate(session);

            Assert.AreEqual(spent, session.EntropySpent, 0.0001f);
        }

        [Test]
        public void Grade_BucketsEntropyUse()
        {
            SpaceShapingSession session = new(Setup(0f, 9f));
            Assert.AreEqual(3, session.Grade());

            session.ApplyZone(Zone(1.5f));
            Assert.AreEqual(2, session.Grade());

            session.ApplyZone(Zone(2f));
            Assert.AreEqual(1, session.Grade());
        }

        private static SpaceZone Zone(float scale) => new(Vector3.zero, Vector3.one, scale, 0f);
    }
}
