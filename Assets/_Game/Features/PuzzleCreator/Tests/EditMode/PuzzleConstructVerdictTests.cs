using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // The verdict on a hand-placed field: today's shipped 01_03 and its stored answer must pass the
    // whole gate, and an empty answer must not.
    public class PuzzleConstructVerdictTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        private const string FieldPath =
            "Assets/_Game/Resources/Chapters/01/Job_01_01_Puzzle.asset";

        [Test]
        public void ShippedAnswer_PassesTheConstructGate()
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract contract = PuzzleJobContracts.ById("01_01");
            PuzzleData field = LoadField();
            PuzzleAnswerAsset answer = LoadAnswer(contract.Id);
            PuzzleSeed seed = contract.CreateSeed();
            try
            {
                PuzzleBakeResult result = PuzzleConstructBake.VerifyAnswer(
                    field, answer.zones, Request(contract, seed, settings));
                Assert.IsTrue(result.Baked, $"{contract.Id}: {result.Status}");
                Assert.Greater(result.Window, 0f);
                Object.DestroyImmediate(result.Puzzle);
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        [Test]
        public void NoAnswer_FailsWithNoSolution()
        {
            SpaceShapingSettings settings = LoadSettings();
            PuzzleJobContract contract = PuzzleJobContracts.ById("01_01");
            PuzzleSeed seed = contract.CreateSeed();
            try
            {
                PuzzleBakeResult result = PuzzleConstructBake.VerifyAnswer(
                    LoadField(), System.Array.Empty<PuzzleZone>(),
                    Request(contract, seed, settings));
                Assert.AreEqual(PuzzleBakeStatus.NoSolution, result.Status);
            }
            finally
            {
                Object.DestroyImmediate(seed);
            }
        }

        private static PuzzleConstructRequest Request(
            PuzzleJobContract contract, PuzzleSeed seed, SpaceShapingSettings settings) => new()
        {
            Contract = contract,
            Brief = PuzzleBrief.From(seed, settings),
            Seed = PuzzleSeedFields.From(seed, settings),
            Field = PuzzleFieldSettings.From(settings),
            Sim = PuzzleSimSettings.From(settings),
            Attempt = 0
        };

        private static SpaceShapingSettings LoadSettings()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            return settings;
        }

        private static PuzzleData LoadField()
        {
            PuzzleData field = AssetDatabase.LoadAssetAtPath<PuzzleData>(FieldPath);
            Assert.IsNotNull(field, FieldPath);
            return field;
        }

        private static PuzzleAnswerAsset LoadAnswer(string id)
        {
            PuzzleAnswerAsset answer =
                AssetDatabase.LoadAssetAtPath<PuzzleAnswerAsset>(PuzzleAnswerAsset.PathFor(id));
            Assert.IsNotNull(answer, PuzzleAnswerAsset.PathFor(id));
            return answer;
        }
    }
}
