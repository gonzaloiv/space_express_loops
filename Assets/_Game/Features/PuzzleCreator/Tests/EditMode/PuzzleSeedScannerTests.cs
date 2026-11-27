using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The contract's generator seam: a Construct contract reaches the constructor, a fleet reaches the
    // fleet lesson, and each retry re-seeds instead of reusing the first field.
    public class PuzzleSeedScannerTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private SpaceShapingSettings settings;
        private PuzzleSeed seed;
        private PuzzleJobContract contract;

        [SetUp]
        public void SetUp()
        {
            settings = AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            contract = PuzzleJobContracts.ById("01_03");
            seed = contract.CreateSeed();
        }

        [TearDown]
        public void TearDown()
        {
            if (seed != null)
                Object.DestroyImmediate(seed);
        }

        [Test]
        public void ScratchContract_IsAConstructAndNotShipped()
        {
            PuzzleJobContract scratch = PuzzleJobContracts.ById("01_01C");
            Assert.IsTrue(scratch.Pending);
            Assert.AreEqual(PuzzleGenerator.Construct, scratch.Generator);
            for (int i = 0; i < PuzzleJobContracts.Enabled.Count; i++)
                Assert.AreNotEqual("01_01C", PuzzleJobContracts.Enabled[i].Id);
        }

        [Test]
        public void ConstructFleet_UsesTheFleetLesson()
        {
            PuzzleJobContract fleet = PuzzleJobContracts.ById("02_07C");
            PuzzleSeed source = fleet.CreateSeed();
            try
            {
                RecordingLesson lesson = new();
                PuzzleSeedScanResult result =
                    PuzzleSeedScanner.Find(fleet, source, settings, 1, lesson);
                Assert.AreEqual(PuzzleBakeStatus.NoSolution, result.Status);
                Assert.AreEqual(1, lesson.SeedValues.Count);
                Assert.AreEqual(fleet.Ships, lesson.ShipCounts[0]);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void ConstructSingle_UsesTheLessonSeam()
        {
            contract.Generator = PuzzleGenerator.Construct;
            RecordingLesson lesson = new();
            PuzzleSeedScanResult result = PuzzleSeedScanner.Find(contract, seed, settings, 1, lesson);
            Assert.AreEqual(PuzzleBakeStatus.NoSolution, result.Status);
            Assert.AreEqual(1, lesson.SeedValues.Count);
        }

        [Test]
        public void BuildFailure_NamesTheStep()
        {
            contract.Generator = PuzzleGenerator.Construct;
            RecordingLesson lesson = new() { Plan = null };
            PuzzleSeedScanResult result = PuzzleSeedScanner.Find(contract, seed, settings, 1, lesson);
            Assert.AreEqual(PuzzleBakeStatus.NoRun, result.Status);
            Assert.IsNotNull(result.Problems);
            StringAssert.Contains(nameof(PuzzleConstructStep.Lesson), result.Problems[0]);
        }

        [Test]
        public void Bake_RetriesWithAFreshStreamPerAttempt()
        {
            RecordingLesson lesson = new();
            PuzzleBakeResult result = PuzzleConstructBake.Bake(Request(lesson), 3);
            Assert.AreEqual(PuzzleBakeStatus.NoSolution, result.Status);
            Assert.AreEqual(3, lesson.SeedValues.Count);
            Assert.AreNotEqual(lesson.SeedValues[0], lesson.SeedValues[1]);
        }

        private PuzzleConstructRequest Request(IPuzzleLesson lesson, int attempt = 0) => new()
        {
            Contract = contract,
            Brief = PuzzleBrief.From(seed, settings),
            Seed = PuzzleSeedFields.From(seed, settings),
            Field = PuzzleFieldSettings.From(settings),
            Sim = PuzzleSimSettings.From(settings),
            Lesson = lesson,
            Attempt = attempt
        };

        private sealed class RecordingLesson : IPuzzleLesson
        {
            public readonly List<int> SeedValues = new();
            public readonly List<int> ShipCounts = new();
            public PuzzleLessonPlan Plan = PuzzleLessonPlan.None;

            public PuzzleLessonPlan Place(PuzzleLessonContext context)
            {
                SeedValues.Add(context.SeedValue);
                ShipCounts.Add(context.Ships != null ? context.Ships.Length : 0);
                return Plan;
            }
        }
    }
}
