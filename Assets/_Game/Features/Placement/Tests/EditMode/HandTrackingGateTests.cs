using NUnit.Framework;

namespace DigitalLove.Game.Placement.Tests.EditMode
{
    public class HandTrackingGateTests
    {
        private const float Step = 0.02f;

        [Test]
        public void BothHands_AreReadyAfterTheSteadyWindow()
        {
            HandTrackingGate gate = new();

            HandTrackingStatus status = gate.Tick(true, true, true, HandTrackingGate.SteadySecs - Step);
            Assert.AreEqual(HandTrackingState.Settling, status.State);
            Assert.IsFalse(status.HintDue);

            status = gate.Tick(true, true, true, Step * 2f);
            Assert.AreEqual(HandTrackingState.Ready, status.State);
            Assert.IsFalse(status.HintDue);
        }

        [Test]
        public void ALostHand_RestartsTheWindow()
        {
            HandTrackingGate gate = new();

            gate.Tick(true, true, true, HandTrackingGate.SteadySecs - Step);
            Assert.AreEqual(HandTrackingState.Missing, gate.Tick(true, true, false, Step).State);
            Assert.AreEqual(HandTrackingState.Settling, gate.Tick(true, true, true, Step).State);

            Assert.AreEqual(HandTrackingState.Ready, gate.Tick(true, true, true, HandTrackingGate.SteadySecs).State);
        }

        [Test]
        public void OneHand_NeverPasses()
        {
            HandTrackingGate gate = new();

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(HandTrackingState.Missing, gate.Tick(true, true, false, Step).State);
        }

        [Test]
        public void HandTrackingOff_NeverPasses()
        {
            HandTrackingGate gate = new();

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(HandTrackingState.Missing, gate.Tick(false, true, true, Step).State);
        }

        [Test]
        public void OffAgainMidWindow_RestartsTheWindow()
        {
            HandTrackingGate gate = new();

            gate.Tick(true, true, true, HandTrackingGate.SteadySecs - Step);
            Assert.AreEqual(HandTrackingState.Missing, gate.Tick(false, true, true, Step).State);
            Assert.AreEqual(HandTrackingState.Settling, gate.Tick(true, true, true, Step).State);

            Assert.AreEqual(HandTrackingState.Ready, gate.Tick(true, true, true, HandTrackingGate.SteadySecs).State);
        }

        [Test]
        public void BothHandsFromTheStart_NeverMakeTheHintDue()
        {
            HandTrackingGate gate = new();

            for (int i = 0; i < 50; i++)
                Assert.IsFalse(gate.Tick(true, true, true, 0.1f).HintDue);

            Assert.AreEqual(HandTrackingState.Ready, gate.Tick(true, true, true, 0.1f).State);
        }

        [Test]
        public void ShortFlicker_DoesNotMakeTheHintDue()
        {
            HandTrackingGate gate = new();

            gate.Tick(true, true, true, 0.1f);
            Assert.IsFalse(gate.Tick(true, true, false, 0.2f).HintDue);
            Assert.IsFalse(gate.Tick(true, true, true, HandTrackingGate.SteadySecs).HintDue);
        }

        [Test]
        public void MissingPastTheDelay_MakesTheHintDue()
        {
            HandTrackingGate gate = new();

            HandTrackingStatus status = gate.Tick(false, true, true, HandTrackingGate.HintDelaySecs + Step);

            Assert.AreEqual(HandTrackingState.Missing, status.State);
            Assert.IsTrue(status.HintDue);
        }

        [Test]
        public void HintClearsWhenHandsReturn()
        {
            HandTrackingGate gate = new();

            gate.Tick(false, true, true, HandTrackingGate.HintDelaySecs + Step);
            HandTrackingStatus status = gate.Tick(true, true, true, Step);

            Assert.AreEqual(HandTrackingState.Settling, status.State);
            Assert.IsFalse(status.HintDue);
        }
    }
}
