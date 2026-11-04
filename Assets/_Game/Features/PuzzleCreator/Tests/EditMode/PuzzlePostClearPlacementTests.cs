using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzlePostClearPlacementTests
    {
        private const string SettingsPath =
            "Assets/_Game/Features/PuzzleCreator/Data/SpaceShapingSettings.asset";

        [Test]
        [Explicit]
        public void Job4_MakeBlockerFatal_ReportsFiveChecks()
        {
            SpaceShapingSettings settings =
                AssetDatabase.LoadAssetAtPath<SpaceShapingSettings>(SettingsPath);
            Assert.IsNotNull(settings, SettingsPath);
            PuzzleJobContract contract = PuzzleJobContracts.All[3];
            PuzzleSeed seed = Seed(contract);
            PuzzleSeedScanResult result =
                PuzzleSeedScanner.Find(seed, settings, contract.Attempts);
            PuzzlePostClearReport report = result.PostClear;

            Assert.IsTrue(report.Tried, $"probe did not run: {report}");
            if (!report.Kept)
                Assert.Fail($"no fatal-blocker placement: {result.Status} {report}");

            Assert.IsTrue(report.Moved || report.Grown, report.ToString());
            Assert.IsTrue(report.MixedBaked, report.ToString());
            Assert.AreEqual(PuzzleSolutionKind.Mixed, report.MixedKind, report.ToString());
            Assert.IsFalse(report.CurveBaked, report.ToString());
            Assert.IsFalse(report.ClearBaked, report.ToString());
            Assert.GreaterOrEqual(report.MixedZones, 2, report.ToString());

            Object.DestroyImmediate(seed);
        }

        private static PuzzleSeed Seed(PuzzleJobContract contract)
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = contract.Seed;
            seed.planetCount = contract.PlanetCount;
            seed.translatable = contract.Translatable;
            seed.startRule = contract.StartRule;
            seed.zones = contract.Zones;
            seed.minRoutes = contract.MinRoutes;
            seed.mechanism = contract.Mechanism;
            seed.hasSun = contract.HasSun;
            seed.sunRule = contract.SunRule;
            seed.sunOnChord = contract.SunOnChord;
            seed.noFail = contract.NoFail;
            seed.noGrade = contract.NoGrade;
            seed.fieldShape = contract.FieldShape;
            seed.resizable = false;
            return seed;
        }
    }
}
