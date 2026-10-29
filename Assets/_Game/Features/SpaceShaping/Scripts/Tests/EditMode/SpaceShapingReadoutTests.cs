using DigitalLove.Game.SpaceShaping.Cube;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class SpaceShapingReadoutTests
    {
        [Test]
        public void Unchecked_ReportsPending_WithoutAward()
        {
            SpaceShapingSession session = new(Setup());
            session.PreviewZone(Zone(1.5f));

            PuzzleReadout readout = PuzzleReadout.From(session, Simulate(session), false);

            Assert.IsFalse(readout.IsChecked);
            Assert.IsFalse(readout.IsSolved);
            Assert.IsFalse(readout.IsFailed);
            Assert.IsTrue(readout.PredictedSolved);
            Assert.AreEqual(0, readout.Grade);
            Assert.AreEqual(0f, readout.EntropySpent, 0.0001f);
            Assert.AreEqual(0.5f, readout.EntropyPending, 0.0001f);
            Assert.AreEqual(0, readout.CommandCount);
        }

        [Test]
        public void CheckedSolved_AwardsGradeFromCommittedEntropy()
        {
            SpaceShapingSession session = new(Setup());
            session.ApplyZone(Zone(1.5f));

            PuzzleReadout readout = PuzzleReadout.From(session, Simulate(session), true);

            Assert.IsTrue(readout.IsChecked);
            Assert.IsTrue(readout.IsSolved);
            Assert.IsFalse(readout.IsFailed);
            Assert.AreEqual(2, readout.Grade);
            Assert.AreEqual(0.5f, readout.EntropySpent, 0.0001f);
            Assert.AreEqual(0f, readout.EntropyPending, 0.0001f);
            Assert.AreEqual(0.5f, readout.EntropyLeft, 0.0001f);
            Assert.AreEqual(1, readout.CommandCount);
        }

        [Test]
        public void CheckedMiss_IsNotFailed()
        {
            SpaceShapingSetup setup = Setup();
            setup.Destination = new Vector3(400f, 0f, 0f);
            SpaceShapingSession session = new(setup);
            session.ApplyZone(Zone(1.5f));

            PuzzleReadout readout = PuzzleReadout.From(session, Simulate(session), true);

            Assert.IsTrue(readout.IsChecked);
            Assert.IsFalse(readout.IsSolved);
            Assert.IsFalse(readout.IsFailed);
            Assert.AreEqual(0, readout.Grade);
        }

        [Test]
        public void OverBudgetMiss_IsFailed()
        {
            SpaceShapingSetup setup = Setup();
            setup.Destination = new Vector3(400f, 0f, 0f);
            SpaceShapingSession session = new(setup);
            session.ApplyZone(Zone(3f));

            PuzzleReadout readout = PuzzleReadout.From(session, Simulate(session), true);

            Assert.IsTrue(readout.IsChecked);
            Assert.IsFalse(readout.IsSolved);
            Assert.IsTrue(readout.IsFailed);
            Assert.AreEqual(0f, readout.EntropyLeft, 0.0001f);
            Assert.AreEqual(0, readout.Grade);
        }

        [Test]
        public void OverBudgetSolved_SolveWins()
        {
            SpaceShapingSession session = new(Setup());
            session.ApplyZone(OffPathZone(3f));

            PuzzleReadout readout = PuzzleReadout.From(session, Simulate(session), true);

            Assert.IsTrue(readout.IsSolved);
            Assert.IsFalse(readout.IsFailed);
            Assert.AreEqual(1, readout.Grade);
            Assert.AreEqual(0f, readout.EntropyLeft, 0.0001f);
        }

        [Test]
        public void Pending_BecomesCommittedOnCommit()
        {
            SpaceShapingSession session = new(Setup());
            session.PreviewZone(Zone(1.5f));

            Assert.AreEqual(0f, session.EntropyCommitted, 0.0001f);
            Assert.AreEqual(0.5f, session.PendingEntropy, 0.0001f);

            session.CommitPreview();

            Assert.AreEqual(0.5f, session.EntropyCommitted, 0.0001f);
            Assert.AreEqual(0f, session.PendingEntropy, 0.0001f);
        }

        [Test]
        public void Grade_UsesCommittedEntropy_NotPreview()
        {
            SpaceShapingSession session = new(Setup());
            session.ApplyZone(Zone(2f));
            Assert.AreEqual(1, session.Grade());

            session.PreviewZone(Zone(3f));
            Assert.AreEqual(1, session.Grade());
        }

        [Test]
        public void CommandCount_CountsCommittedZones()
        {
            SpaceShapingSession session = new(Setup());
            session.ApplyZone(Zone(1.5f));
            session.ApplyZone(new SpaceZone(new Vector3(1f, 0f, 0f), Vector3.one, 1.2f, 0f));

            PuzzleReadout readout = PuzzleReadout.From(session, Simulate(session), true);

            Assert.AreEqual(2, readout.CommandCount);
        }

        [Test]
        public void ZeroBudget_IsNotFailed()
        {
            SpaceShapingSetup setup = Setup();
            setup.EntropyBudget = 0f;
            SpaceShapingSession session = new(setup);
            session.ApplyZone(Zone(1.5f));

            PuzzleReadout readout = PuzzleReadout.From(session, Simulate(session), true);

            Assert.IsFalse(session.IsOverBudget);
            Assert.IsFalse(readout.IsFailed);
        }

        private static SpaceShapingSetup Setup() => new()
        {
            Origin = new Vector3(-4f, 0f, 0f),
            Destination = new Vector3(4f, 0f, 0f),
            MaxSteps = 1200,
            LaunchSpeed = 1f,
            Tmin = 0f,
            Tmax = 9f,
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

        private static SpaceZone Zone(float scale) => new(Vector3.zero, Vector3.one, scale, 0f);

        private static SpaceZone OffPathZone(float scale) =>
            new(new Vector3(100f, 0f, 0f), Vector3.one, scale, 0f);
    }
}
