using DigitalLove.Game.Spatial;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class WellPullTests
    {
        private const float Radius = 0.04f;

        private static NativeArray<T> Native<T>(params T[] items) where T : struct =>
            new(items, Allocator.Temp);

        private static WellPull Pull(
            NativeArray<SpaceSource> sources, NativeArray<SpaceSource> baseline,
            float gain = 0.07f, float maxFraction = 0.6f) =>
            new(sources.AsReadOnly(), baseline.AsReadOnly(), gain, maxFraction);

        private static Vector3 PullOne(
            float strength, Vector3 point, float gain = 0.07f, float maxFraction = 0.6f)
        {
            using NativeArray<SpaceSource> sources = Native(new SpaceSource(Vector3.zero, Radius, strength));
            using NativeArray<SpaceSource> none = Native<SpaceSource>();
            return Pull(sources, none, gain, maxFraction).Pull(point);
        }

        [Test]
        public void NoSources_LeavesThePoint()
        {
            using NativeArray<SpaceSource> none = Native<SpaceSource>();
            Vector3 point = new(0.2f, 0.1f, -0.3f);

            Assert.AreEqual(point, Pull(none, none).Pull(point));
        }

        [Test]
        public void APoint_MovesTowardTheSource()
        {
            Vector3 point = new(0.2f, 0f, 0f);

            Vector3 pulled = PullOne(0.0004f, point);

            Assert.Less(pulled.x, point.x);
            Assert.AreEqual(0f, pulled.y, 1e-6f);
            Assert.AreEqual(0f, pulled.z, 1e-6f);
        }

        [Test]
        public void AStrongerSource_PullsFurther()
        {
            Vector3 point = new(0.3f, 0f, 0f);

            float weak = point.x - PullOne(0.0001f, point).x;
            float strong = point.x - PullOne(0.001f, point).x;

            Assert.Greater(strong, weak);
        }

        [Test]
        public void APull_NeverReachesTheSurface()
        {
            Vector3 point = new(Radius + 0.01f, 0f, 0f);

            Vector3 pulled = PullOne(1f, point, gain: 10f, maxFraction: 0.6f);

            Assert.Greater(pulled.x, Radius);
            Assert.AreEqual(Radius + 0.004f, pulled.x, 1e-5f);
        }

        // Job 1's case: a strong sun behind a small planet. Each pull is capped alone; the sum must not
        // carry a node 1 cm outside the planet through it.
        [Test]
        public void ASunBehindAPlanet_CannotDragANodeThroughIt()
        {
            SpaceSource sun = new(Vector3.zero, 0.06f, 1f);
            SpaceSource planet = new(new Vector3(0.3f, 0f, 0f), 0.053f, 0.0002f);
            using NativeArray<SpaceSource> sources = Native(sun, planet);
            using NativeArray<SpaceSource> none = Native<SpaceSource>();
            Vector3 point = new(0.3f + 0.053f + 0.01f, 0f, 0f);

            Vector3 pulled = Pull(sources, none).Pull(point);

            float fromPlanet = Vector3.Distance(pulled, planet.Position);
            Assert.GreaterOrEqual(fromPlanet, 0.053f + 0.4f * 0.01f - 1e-5f);
            Assert.Less(pulled.x, point.x);
        }

        [Test]
        public void APointInsideASource_StaysPut()
        {
            Vector3 point = new(Radius * 0.5f, 0f, 0f);

            Assert.AreEqual(point, PullOne(1f, point, gain: 10f));
        }

        [Test]
        public void APointInsideOneSource_StaysPut_WhileAnotherPulls()
        {
            SpaceSource sun = new(Vector3.zero, 0.06f, 1f);
            SpaceSource planet = new(new Vector3(0.3f, 0f, 0f), 0.053f, 0.0002f);
            using NativeArray<SpaceSource> sources = Native(sun, planet);
            using NativeArray<SpaceSource> none = Native<SpaceSource>();
            Vector3 point = new(0.32f, 0f, 0f);

            Assert.AreEqual(point, Pull(sources, none).Pull(point));
        }

        [Test]
        public void AFullCap_StillStopsShortOfTheSurface()
        {
            Vector3 point = new(Radius + 0.01f, 0f, 0f);

            Vector3 pulled = PullOne(1f, point, gain: 10f, maxFraction: 1f);

            Assert.Greater(pulled.x, Radius + 1e-4f);
        }

        [Test]
        public void AnUnchangedField_DrawsNoChange()
        {
            using NativeArray<SpaceSource> field = Native(new SpaceSource(Vector3.zero, Radius, 0.0004f));
            Vector3 point = new(0.2f, 0f, 0f);

            Assert.AreEqual(point, Pull(field, field).Pull(point));
        }

        [Test]
        public void AGrownSource_DrawsANewWell_AndAShrunkOneFlattens()
        {
            using NativeArray<SpaceSource> start = Native(new SpaceSource(Vector3.zero, Radius, 0.0004f));
            using NativeArray<SpaceSource> grown = Native(new SpaceSource(Vector3.zero, Radius, 0.004f));
            using NativeArray<SpaceSource> shrunk = Native(new SpaceSource(Vector3.zero, Radius, 0.0001f));
            Vector3 point = new(0.2f, 0f, 0f);

            Assert.Less(Pull(grown, start).Pull(point).x, point.x);
            Assert.Greater(Pull(shrunk, start).Pull(point).x, point.x);
        }

        [Test]
        public void WellsOff_GiveNoPull()
        {
            LatticeWells wells = new() { enabled = false };
            using NativeArray<SpaceSource> sources = Native(new SpaceSource(Vector3.zero, Radius, 1f));
            Vector3 point = new(0.2f, 0f, 0f);

            Assert.AreEqual(point, wells.Pull(sources, sources).Pull(point));
        }

        [Test]
        public void TheWarp_AppliesWellsAfterTheZones()
        {
            using NativeArray<SpaceSource> sources = Native(new SpaceSource(Vector3.zero, Radius, 0.0004f));
            using NativeArray<SpaceSource> none = Native<SpaceSource>();
            using NativeArray<SpaceZone> zones = Native<SpaceZone>();
            WellPull wells = Pull(sources, none);
            Vector3 point = new(0.2f, 0f, 0f);

            LatticeWarp warp = new(zones.AsReadOnly(), wells);

            Assert.AreEqual(wells.Pull(point), warp.Warp(point));
            Assert.AreEqual(point, new LatticeWarp(zones.AsReadOnly(), Pull(none, none)).Warp(point));
        }
    }
}
