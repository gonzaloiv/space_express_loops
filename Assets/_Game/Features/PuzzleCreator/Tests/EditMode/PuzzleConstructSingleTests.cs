using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleConstructSingleTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        [Test]
        public void Clear_BuildsAndBakes()
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract contract = PuzzleJobContracts.ById("01_03");
            PuzzleSeed seed = contract.CreateSeed();
            try
            {
                PuzzleBakeResult result = PuzzleConstructBake.Bake(
                    Request(contract, seed, settings, new PuzzleClearLesson()));
                Assert.IsTrue(result.Baked, $"{contract.Id}: {result.Status}");
                Assert.AreEqual(PuzzleSolutionKind.Clear, result.Kind);
                Assert.Greater(result.Window, 0f);
                Object.DestroyImmediate(result.Puzzle);
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        [Test]
        public void Curve_BuildsAndBakes()
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract contract = PuzzleJobContracts.ById("01_01");
            PuzzleSeed seed = contract.CreateSeed();
            try
            {
                PuzzleBakeResult result = PuzzleConstructBake.Bake(
                    Request(contract, seed, settings, new PuzzleCurveLesson()));
                Assert.IsTrue(result.Baked, $"{contract.Id}: {result.Status}");
                Assert.AreEqual(PuzzleSolutionKind.Curve, result.Kind);
                Assert.Greater(result.Window, 0f);
                Object.DestroyImmediate(result.Puzzle);
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        [Test, Explicit]
        public void Clear_MeetsTheContract_On20Seeds() =>
            Rate("01_03", new PuzzleClearLesson());

        [Test]
        public void SingleCurve_RejectsAClearShortcut()
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract contract = ShortcutContract();
            PuzzleSeed seed = contract.CreateSeed();
            try
            {
                PuzzleData field = LoadField("01_03");
                Assert.IsTrue(PuzzleConstructBake.ClearShortcut(
                    field, Request(contract, seed, settings, null)));
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        [Test, Explicit]
        public void Curve_MeetsTheContract_On20Seeds() =>
            Rate("01_01", new PuzzleCurveLesson());

        private static void Rate(string id, IPuzzleLesson lesson)
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract contract = PuzzleJobContracts.ById(id);
            int passed = 0;
            for (int i = 0; i < 20; i++)
            {
                if (Passes(contract, settings, lesson, i))
                    passed++;
            }

            Debug.Log($"{id}: {passed}/20 constructed");
            Assert.Greater(passed, 10, $"{id}: {passed}/20");
        }

        private static bool Passes(
            PuzzleJobContract contract, SpaceShapingSettings settings, IPuzzleLesson lesson,
            int index)
        {
            PuzzleSeed seed = contract.CreateSeed();
            try
            {
                seed.seed = contract.Seed + index;
                PuzzleBakeResult result = PuzzleConstructBake.Bake(
                    Request(contract, seed, settings, lesson));
                if (!result.Baked)
                {
                    Debug.Log($"{contract.Id} fail {result.Status}");
                    return false;
                }

                Object.DestroyImmediate(result.Puzzle);
                return true;
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        private static PuzzleJobContract ShortcutContract() => new()
        {
            Id = "01_01M", Seed = 141, Attempts = 4, PlanetCount = 6,
            FieldShape = PuzzleFieldShape.Ring, HasSun = true, SunRule = PuzzleSunRule.OutOfZone,
            MinBlockers = 1, Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
            Translatable = true, NoFail = true, NoGrade = true, NoIntro = true, GrowOnly = false
        };

        private static PuzzleData LoadField(string id)
        {
            string chapter = id.Substring(0, 2);
            string path = $"Assets/_Game/Resources/Chapters/{chapter}/Job_{id}_Puzzle.asset";
            PuzzleData data = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            Assert.IsNotNull(data, path);
            return data;
        }

        private static PuzzleConstructRequest Request(
            PuzzleJobContract contract, PuzzleSeed seed, SpaceShapingSettings settings,
            IPuzzleLesson lesson) => new()
        {
            Contract = contract,
            Brief = PuzzleBrief.From(seed, settings),
            Seed = PuzzleSeedFields.From(seed, settings),
            Field = PuzzleFieldSettings.From(settings),
            Sim = PuzzleSimSettings.From(settings),
            Lesson = lesson,
            Attempt = 0
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
