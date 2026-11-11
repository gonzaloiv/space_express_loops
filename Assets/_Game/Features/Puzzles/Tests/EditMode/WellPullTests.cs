using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class WellPullTests
    {
        private const float Radius = 0.04f;

        private static WellPull Pull(float strength, float gain = 0.07f, float maxFraction = 0.6f) =>
            new(new[] { new SpaceSource(Vector3.zero, Radius, strength) }, gain, maxFraction);

        [Test]
        public void NoSources_LeavesThePoint()
        {
            WellPull pull = new(null, 0.07f, 0.6f);
            Vector3 point = new(0.2f, 0.1f, -0.3f);

            Assert.AreEqual(point, pull.Pull(point));
        }

        [Test]
        public void APoint_MovesTowardTheSource()
        {
            Vector3 point = new(0.2f, 0f, 0f);

            Vector3 pulled = Pull(0.0004f).Pull(point);

            Assert.Less(pulled.x, point.x);
            Assert.AreEqual(0f, pulled.y, 1e-6f);
            Assert.AreEqual(0f, pulled.z, 1e-6f);
        }

        [Test]
        public void AStrongerSource_PullsFurther()
        {
            Vector3 point = new(0.3f, 0f, 0f);

            float weak = point.x - Pull(0.0001f).Pull(point).x;
            float strong = point.x - Pull(0.001f).Pull(point).x;

            Assert.Greater(strong, weak);
        }

        [Test]
        public void APull_NeverReachesTheSurface()
        {
            Vector3 point = new(Radius + 0.01f, 0f, 0f);

            Vector3 pulled = Pull(1f, gain: 10f, maxFraction: 0.6f).Pull(point);

            Assert.Greater(pulled.x, Radius);
            Assert.AreEqual(Radius + 0.004f, pulled.x, 1e-5f);
        }

        [Test]
        public void APointInsideASource_StaysPut()
        {
            Vector3 point = new(Radius * 0.5f, 0f, 0f);

            Assert.AreEqual(point, Pull(1f, gain: 10f).Pull(point));
        }

        [Test]
        public void TheWarp_AppliesWellsAfterTheZones()
        {
            WellPull wells = Pull(0.0004f);
            Vector3 point = new(0.2f, 0f, 0f);

            LatticeWarp warp = new(null, wells);

            Assert.AreEqual(wells.Pull(point), warp.Warp(point));
            Assert.AreEqual(point, new LatticeWarp(null).Warp(point));
        }
    }
}
