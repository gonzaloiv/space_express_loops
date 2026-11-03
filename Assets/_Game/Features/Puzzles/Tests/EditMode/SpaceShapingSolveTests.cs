using DigitalLove.Game.SpaceShaping.Cube;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class SpaceShapingSolveTests
    {
        private static SpaceShapingSetup Setup() => new()
        {
            Origin = new Vector3(-4f, 0f, 0f),
            Destination = new Vector3(4f, 0f, 0f),
            MaxSteps = 1200,
            LaunchSpeed = 1f,
            EntropyBudget = 1f,
            Sim = new PuzzleSimSettings
            {
                CaptureRadius = 0.6f,
                StepSecs = 1f / 120f,
                SourceStrengthScale = 2f,
                SeparationScale = 1.2f
            },
            Sources = System.Array.Empty<SourceSpec>()
        };

        private static TrajectoryResult Simulate(SpaceShapingSession session) =>
            session.Simulate();

        [Test]
        public void Arrival_IsSolved()
        {
            SpaceShapingSession session = new(Setup());
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Arrive, result.Outcome);
            Assert.IsTrue(session.IsSolved(result));
        }

        [Test]
        public void Miss_IsNotSolved()
        {
            SpaceShapingSetup setup = Setup();
            setup.Destination = new Vector3(400f, 0f, 0f);
            SpaceShapingSession session = new(setup);
            TrajectoryResult result = Simulate(session);

            Assert.AreEqual(TrajectoryOutcome.Miss, result.Outcome);
            Assert.IsFalse(session.IsSolved(result));
        }

        [Test]
        public void Crash_IsNotSolved()
        {
            SpaceShapingSetup setup = Setup();
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
        public void Reset_ClearsCommandsAndRestoresSources()
        {
            SpaceShapingSetup setup = Setup();
            setup.Sources = new[]
            {
                new SourceSpec
                {
                    Home = Vector3.one,
                    InitialScale = 2f,
                    MinScale = 1f,
                    MaxScale = 3f
                }
            };
            SpaceShapingSession session = new(setup);
            session.ApplyZone(new SpaceZone(Vector3.one, Vector3.one, 1.5f, 0f));

            Assert.Greater(session.EntropyCommitted, 0f);
            Assert.AreNotEqual(2f, session.GetScale(0));

            session.Reset();

            Assert.AreEqual(0, session.CommandCount);
            Assert.AreEqual(0f, session.EntropyCommitted, 0.0001f);
            Assert.AreEqual(0f, session.PendingEntropy, 0.0001f);
            Assert.AreEqual(2f, session.GetScale(0), 0.0001f);
            Assert.AreEqual(Vector3.one, session.GetPosition(0));
        }

        [Test]
        public void Entropy_IsChargedPerModification_NotPerSimulation()
        {
            SpaceShapingSession session = new(Setup());
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
            SpaceShapingSession session = new(Setup());
            Assert.AreEqual(3, session.Grade());

            session.ApplyZone(Zone(1.5f));
            Assert.AreEqual(2, session.Grade());

            session.ApplyZone(Zone(2f));
            Assert.AreEqual(1, session.Grade());
        }

        private static SpaceZone Zone(float scale) => new(Vector3.zero, Vector3.one, scale, 0f);
    }
}
