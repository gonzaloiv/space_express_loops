using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // A moved decoy re-samples the field's own shape, so Ring, Spiral and Cluster keep their layouts
    // instead of gaining a planet in the middle.
    public class PuzzleConstructDecoyTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";
        private const int ShapeSeed = 1234;

        [Test]
        public void RelocatedDecoy_StaysOnTheRing() => StaysOnShape("01_03");

        [Test]
        public void RelocatedDecoy_StaysOnTheSpiral() => StaysOnShape("01_04");

        [Test]
        public void RelocatedDecoy_StaysOnTheCluster() => StaysOnShape("01_06");

        [Test]
        public void Build_KeepsEveryDecoyOnTheShape()
        {
            string[] ids = { "01_03", "01_04", "01_06" };
            for (int i = 0; i < ids.Length; i++)
                AssertBuiltOnShape(ids[i]);
        }

        private static void StaysOnShape(string id)
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract contract = PuzzleJobContracts.ById(id);
            PuzzleSeed seed = contract.CreateSeed();
            try
            {
                PuzzleConstructRequest request = Request(contract, seed, settings);
                PuzzleConstructedField field = PuzzleConstruct.Build(request);
                Assert.IsNotNull(field, id);
                int index = FirstUnprotected(field);
                Assert.GreaterOrEqual(index, 0, id);

                PuzzleDecoyField decoys = Decoys(field, request);
                float radius = decoys.Planets[index].radius;
                Vector3 before = decoys.Planets[index].position;
                Assert.IsTrue(PuzzleConstructDecoys.TryRelocate(decoys, index, ShapeSeed), id);
                Assert.AreNotEqual(before, decoys.Planets[index].position, id);
                Assert.AreEqual(radius, decoys.Planets[index].radius, 0.0001f, id);
                Assert.IsTrue(
                    OnShape(request, ShapeSeed, decoys.Planets[index].position), id);
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        private static void AssertBuiltOnShape(string id)
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract contract = PuzzleJobContracts.ById(id);
            PuzzleSeed seed = contract.CreateSeed();
            try
            {
                PuzzleConstructRequest request = Request(contract, seed, settings);
                PuzzleConstructedField field = PuzzleConstruct.Build(request);
                Assert.IsNotNull(field, id);
                int baseSeed = PuzzleConstructSeeds.Value(request.Seed.Seed, request.Attempt);
                for (int i = 0; i < field.Planets.Length; i++)
                {
                    if (field.Protected[i])
                        continue;

                    Assert.IsTrue(
                        OnAnyShape(request, baseSeed, field.Planets.Length,
                            field.Planets[i].position),
                        $"{id} decoy {i}");
                }
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        private static bool OnAnyShape(
            PuzzleConstructRequest request, int seed, int count, Vector3 position)
        {
            if (OnShape(request, seed, position))
                return true;
            for (int i = 0; i < count; i++)
            {
                if (OnShape(request, seed + i, position))
                    return true;
            }

            return false;
        }

        private static bool OnShape(
            PuzzleConstructRequest request, int seed, Vector3 position)
        {
            PuzzleFieldPlanet[] shape = PuzzleConstructShape.Generate(request, seed);
            for (int i = 0; i < shape.Length; i++)
            {
                if ((shape[i].position - position).sqrMagnitude < 0.000001f)
                    return true;
            }

            return false;
        }

        private static PuzzleDecoyField Decoys(
            PuzzleConstructedField field, PuzzleConstructRequest request) => new()
        {
            Planets = (PuzzleFieldPlanet[])field.Planets.Clone(),
            Protected = field.Protected,
            Request = request,
            Origin = field.Origin,
            Destination = field.Destination,
            Ships = field.Ships
        };

        private static int FirstUnprotected(PuzzleConstructedField field)
        {
            for (int i = 0; i < field.Planets.Length; i++)
            {
                if (!field.Protected[i])
                    return i;
            }

            return -1;
        }

        private static PuzzleConstructRequest Request(
            PuzzleJobContract contract, PuzzleSeed seed, SpaceShapingSettings settings) => new()
        {
            Contract = contract,
            Brief = PuzzleBrief.From(seed, settings),
            Seed = PuzzleSeedFields.From(seed, settings),
            Field = PuzzleFieldSettings.From(settings),
            Sim = PuzzleSimSettings.From(settings),
            Lesson = null
        };

        private static SpaceShapingSettings LoadSettings()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            return settings;
        }
    }
}
