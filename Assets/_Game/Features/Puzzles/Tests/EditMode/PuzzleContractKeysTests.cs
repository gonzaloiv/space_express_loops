using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class PuzzleContractKeysTests
    {
        [Test]
        public void Keys_DeriveFromTheLevelId()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.id = "01_02";

            Assert.AreEqual("level_01_02_init_client", puzzle.ClientKey);
            Assert.AreEqual("level_01_02_init_cargo", puzzle.CargoKey);
            Assert.AreEqual("level_01_02_init_manual_entry", puzzle.ManualKey);

            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void IntroLinesKey_GathersTheNumberedLinesOnly()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.id = "01_04";

            StringAssert.Contains(puzzle.IntroLinesKey, "level_01_04_init_01");
            StringAssert.Contains(puzzle.IntroLinesKey, "level_01_04_init_02");
            StringAssert.DoesNotContain(puzzle.IntroLinesKey, puzzle.ClientKey);
            StringAssert.DoesNotContain(puzzle.IntroLinesKey, puzzle.CargoKey);
            StringAssert.DoesNotContain(puzzle.IntroLinesKey, puzzle.ManualKey);

            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void ManualVersion_IsTheChapterAndTheJob()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.id = "02_04";

            Assert.AreEqual("2.0.4", puzzle.ManualVersion);
            Assert.AreEqual("chapter_02_manual_release", puzzle.ManualReleaseKey);

            Object.DestroyImmediate(puzzle);
        }
    }
}
