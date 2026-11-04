using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleBlockerPlacementTests
    {
        [Test]
        public void Apply_SunCurve_PlacesOneBlockerOnThePinnedRun()
        {
            PuzzleSeed seed = Seed(PuzzleMechanism.Curve, PuzzleSunRule.CentredGrown);
            SpaceShapingSettings settings = Settings();
            PuzzlePlanetListAsset planets = Planets(
                V(-0.5f, 0.2f), V(0.5f, 0.2f), V(0f, 0.5f), V(0.3f, 0.5f));
            Vector3[] before = Positions(planets);

            PuzzleBlockerPlacement.Apply(planets, seed, settings);

            Assert.IsTrue(planets.hasCandidate);
            Assert.AreEqual(1, Moved(planets, before));
            Assert.AreEqual(
                1, BlockersOnRun(planets, planets.candidateOrigin, planets.candidateDestination));
            Assert.IsFalse(SunOnChord(planets.candidateOrigin, planets.candidateDestination));
            Cleanup(planets, seed, settings);
        }

        [Test]
        public void Apply_ClearOutOfZone_PlacesTwoBlockers()
        {
            PuzzleSeed seed = Seed(PuzzleMechanism.Clear, PuzzleSunRule.OutOfZone);
            SpaceShapingSettings settings = Settings();
            PuzzlePlanetListAsset planets = Planets(
                V(-0.5f, 0.2f), V(0.5f, 0.2f), V(0f, 0.5f), V(0.3f, 0.5f), V(0.5f, 0.5f));
            Vector3[] before = Positions(planets);

            PuzzleBlockerPlacement.Apply(planets, seed, settings);

            Assert.IsTrue(planets.hasCandidate);
            Assert.AreEqual(2, Moved(planets, before));
            Cleanup(planets, seed, settings);
        }

        [Test]
        public void Apply_ClearOutOfZone_KeepsBlockersOffTheSun()
        {
            PuzzleSeed seed = Seed(PuzzleMechanism.Clear, PuzzleSunRule.OutOfZone);
            SpaceShapingSettings settings = Settings();
            PuzzlePlanetListAsset planets = Planets(
                V(-0.5f, 0f), V(0.5f, 0f), V(0f, 0.5f), V(0.3f, 0.5f), V(0.5f, 0.5f));
            Vector3[] before = Positions(planets);

            PuzzleBlockerPlacement.Apply(planets, seed, settings);

            Assert.IsTrue(planets.hasCandidate);
            Assert.IsFalse(SunOnChord(planets.candidateOrigin, planets.candidateDestination));
            Assert.AreEqual(2, Moved(planets, before));
            for (int i = 0; i < planets.planets.Length; i++)
            {
                if ((planets.planets[i].position - before[i]).sqrMagnitude < 0.000001f)
                    continue;

                Assert.GreaterOrEqual(
                    Vector3.Distance(Vector3.zero, planets.planets[i].position), 0.11f);
            }
            Cleanup(planets, seed, settings);
        }

        [Test]
        public void Apply_Mixed_PlacesBlockerAndWell()
        {
            PuzzleSeed seed = Seed(PuzzleMechanism.Mixed, PuzzleSunRule.OutOfZone);
            SpaceShapingSettings settings = Settings();
            PuzzlePlanetListAsset planets = Planets(
                V(-0.5f, 0.2f), V(0.5f, 0.2f), V(-0.3f, 0.5f), V(0f, 0.5f),
                V(0.3f, 0.5f), V(0.5f, 0.5f));
            Vector3[] before = Positions(planets);

            PuzzleBlockerPlacement.Apply(planets, seed, settings);

            Assert.IsTrue(planets.hasCandidate);
            Assert.AreEqual(2, Moved(planets, before));
            Assert.GreaterOrEqual(
                BlockersOnRun(planets, planets.candidateOrigin, planets.candidateDestination), 1);
            Cleanup(planets, seed, settings);
        }

        [Test]
        public void Apply_NoSun_DoesNotPlace()
        {
            PuzzleSeed seed = Seed(PuzzleMechanism.Curve, PuzzleSunRule.Any, hasSun: false);
            SpaceShapingSettings settings = Settings();
            settings.sunRadius = 0f;
            PuzzlePlanetListAsset planets = Planets(
                V(-0.5f, 0.2f), V(0.5f, 0.2f), V(0f, 0.5f));
            Vector3[] before = Positions(planets);

            PuzzleBlockerPlacement.Apply(planets, seed, settings);

            Assert.IsFalse(planets.hasCandidate);
            Assert.AreEqual(0, Moved(planets, before));
            Cleanup(planets, seed, settings);
        }

        private static PuzzleSeed Seed(
            PuzzleMechanism mechanism, PuzzleSunRule sunRule, bool hasSun = true)
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.mechanism = mechanism;
            seed.sunRule = sunRule;
            seed.hasSun = hasSun;
            seed.planetCount = 6;
            return seed;
        }

        private static SpaceShapingSettings Settings()
        {
            SpaceShapingSettings settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            settings.runDistance = 0.7f;
            settings.endpointClearance = 0.033f;
            settings.sunRadius = 0.06f;
            settings.separationScale = 1.2f;
            settings.planetRadiusMin = 0.006f;
            settings.planetRadiusMax = 0.06f;
            return settings;
        }

        private static PuzzlePlanetListAsset Planets(params Vector3[] positions)
        {
            PuzzlePlanetListAsset planets = ScriptableObject.CreateInstance<PuzzlePlanetListAsset>();
            planets.planets = new PuzzleFieldPlanet[positions.Length];
            for (int i = 0; i < positions.Length; i++)
                planets.planets[i] = new PuzzleFieldPlanet(positions[i], 0.05f);

            return planets;
        }

        private static Vector3 V(float x, float y) => new(x, y, 0f);

        private static Vector3[] Positions(PuzzlePlanetListAsset planets)
        {
            Vector3[] positions = new Vector3[planets.planets.Length];
            for (int i = 0; i < positions.Length; i++)
                positions[i] = planets.planets[i].position;

            return positions;
        }

        private static int Moved(PuzzlePlanetListAsset planets, Vector3[] before)
        {
            int moved = 0;
            for (int i = 0; i < planets.planets.Length; i++)
                if ((planets.planets[i].position - before[i]).sqrMagnitude > 0.000001f)
                    moved++;

            return moved;
        }

        private static int BlockersOnRun(
            PuzzlePlanetListAsset planets, Vector3 origin, Vector3 destination)
        {
            int count = 0;
            for (int i = 0; i < planets.planets.Length; i++)
            {
                Vector3 position = planets.planets[i].position;
                if (IsEndpoint(position, origin, destination))
                    continue;

                if (PuzzleRunGeometry.DistanceToSegment(position, origin, destination)
                    < planets.planets[i].radius * 2f)
                    count++;
            }

            return count;
        }

        private static bool SunOnChord(Vector3 origin, Vector3 destination) =>
            PuzzleRunGeometry.DistanceToSegment(Vector3.zero, origin, destination) < 0.12f;

        private static bool IsEndpoint(Vector3 position, Vector3 origin, Vector3 destination) =>
            (position - origin).sqrMagnitude < 0.000001f
            || (position - destination).sqrMagnitude < 0.000001f;

        private static void Cleanup(
            PuzzlePlanetListAsset planets, PuzzleSeed seed, SpaceShapingSettings settings)
        {
            Object.DestroyImmediate(planets);
            Object.DestroyImmediate(seed);
            Object.DestroyImmediate(settings);
        }
    }
}
