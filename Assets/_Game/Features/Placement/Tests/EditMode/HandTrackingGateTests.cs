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

            Assert.IsFalse(gate.Tick(true, true, true, HandTrackingGate.SteadySecs - Step));
            Assert.IsTrue(gate.Tick(true, true, true, Step * 2f));
        }

        [Test]
        public void ALostHand_RestartsTheWindow()
        {
            HandTrackingGate gate = new();

            gate.Tick(true, true, true, HandTrackingGate.SteadySecs - Step);
            gate.Tick(true, true, false, Step);

            Assert.IsFalse(gate.Tick(true, true, true, Step));
            Assert.IsTrue(gate.Tick(true, true, true, HandTrackingGate.SteadySecs));
        }

        [Test]
        public void OneHand_NeverPasses()
        {
            HandTrackingGate gate = new();

            for (int i = 0; i < 100; i++)
                Assert.IsFalse(gate.Tick(true, true, false, Step));
        }

        [Test]
        public void HandTrackingOff_NeverPasses()
        {
            HandTrackingGate gate = new();

            for (int i = 0; i < 100; i++)
                Assert.IsFalse(gate.Tick(false, true, true, Step));
        }

        [Test]
        public void OffAgainMidWindow_RestartsTheWindow()
        {
            HandTrackingGate gate = new();

            gate.Tick(true, true, true, HandTrackingGate.SteadySecs - Step);
            gate.Tick(false, true, true, Step);

            Assert.IsFalse(gate.Tick(true, true, true, Step));
            Assert.IsTrue(gate.Tick(true, true, true, HandTrackingGate.SteadySecs));
        }
    }
}
