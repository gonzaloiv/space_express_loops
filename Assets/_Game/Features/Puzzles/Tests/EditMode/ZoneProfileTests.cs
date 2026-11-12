using DigitalLove.Game.Spatial;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class ZoneProfileTests
    {
        private static SpaceZone Zone(float scale = 2f) =>
            new(Vector3.zero, Vector3.one * 0.5f, scale, 0.5f);

        [Test]
        public void OutsideAndOnTheFace_TheZoneDoesNothing()
        {
            SpaceZone zone = Zone();

            Assert.AreEqual(0f, zone.InsideFactor(new Vector3(0.5f, 0f, 0f)), 1e-5f);
            Assert.AreEqual(0f, zone.InsideFactor(new Vector3(0.8f, 0.1f, 0f)), 1e-5f);
            Assert.AreEqual(1f, zone.FactorAt(new Vector3(0.8f, 0.1f, 0f)), 1e-5f);
        }

        [Test]
        public void TheCore_GetsTheFullScale()
        {
            SpaceZone zone = Zone();

            Assert.AreEqual(1f, zone.InsideFactor(Vector3.zero), 1e-5f);
            Assert.AreEqual(2f, zone.FactorAt(Vector3.zero), 1e-5f);
        }

        [Test]
        public void TheRamp_HasNoKinkAtTheFaceOrTheCore()
        {
            SpaceZone zone = Zone();
            const float h = 0.0005f;

            // Slope is ~0 just inside the face and just outside the core: the ramp eases in and out.
            float atFace = Slope(zone, 0.5f - h, h);
            float atCore = Slope(zone, 0.25f + h, h);
            float mid = Slope(zone, 0.375f, h);

            Assert.Less(Mathf.Abs(atFace), Mathf.Abs(mid) * 0.05f);
            Assert.Less(Mathf.Abs(atCore), Mathf.Abs(mid) * 0.05f);
        }

        [Test]
        public void TheDiagonal_HasNoCrease()
        {
            SpaceZone zone = Zone();
            const float h = 0.001f;
            Vector3 onDiagonal = new(0.4f, 0.4f, 0f);

            // Across the diagonal the factor changes smoothly: the slopes either side match.
            float left = zone.InsideFactor(onDiagonal) - zone.InsideFactor(onDiagonal + new Vector3(-h, h, 0f));
            float right = zone.InsideFactor(onDiagonal + new Vector3(h, -h, 0f)) - zone.InsideFactor(onDiagonal);

            Assert.AreEqual(left, right, 1e-4f);
        }

        [Test]
        public void TheLattice_DrawsExactlyWhatThePhysicsApplies()
        {
            SpaceZone zone = Zone(1.6f);
            LatticeWarp warp = new(new[] { zone });
            Vector3[] positions = { new(0.3f, 0.1f, -0.2f) };
            float[] scales = { 1f };
            SourceSpec[] specs = { default };

            zone.ApplyTo(positions, scales, specs);

            Assert.AreEqual(positions[0], warp.Warp(new Vector3(0.3f, 0.1f, -0.2f)));
        }

        private static float Slope(SpaceZone zone, float x, float h) =>
            (zone.InsideFactor(new Vector3(x + h, 0f, 0f)) - zone.InsideFactor(new Vector3(x - h, 0f, 0f))) / (2f * h);
    }
}
