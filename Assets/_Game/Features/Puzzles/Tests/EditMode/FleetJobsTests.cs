using DigitalLove.Casual.Levels;
using DigitalLove.Game.SpaceShaping.Creator;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class FleetJobsTests
    {
        private const string ChapterOnePath = "Assets/_Game/Resources/Chapters/01.asset";
        private const string ChapterTwoPath = "Assets/_Game/Resources/Chapters/02.asset";
        private const string SelectorPath = "Assets/_Game/Resources/Chapters/LevelSelector.asset";

        [Test]
        public void ChapterTwo_IsTheChapterAfterChapterOne()
        {
            ChapterData one = LoadChapter(ChapterOnePath);
            ChapterData two = LoadChapter(ChapterTwoPath);
            Assert.AreEqual("02", two.id);

            LevelSelector selector = AssetDatabase.LoadAssetAtPath<LevelSelector>(SelectorPath);
            Assert.IsNotNull(selector, SelectorPath);

            for (int i = 0; i < two.levels.Length; i++)
            {
                LevelData level = selector.GetLevelByGlobalIndex<LevelData>(one.levels.Length + i);
                Assert.IsNotNull(level, two.levels[i].id);
                Assert.AreEqual(two.levels[i].id, level.id);
            }
        }

        [Test]
        public void ChapterTwo_LevelsAreInPlayOrder()
        {
            ChapterData two = LoadChapter(ChapterTwoPath);
            for (int i = 1; i < two.levels.Length; i++)
            {
                int previous = LevelData.AsIndex(two.levels[i - 1].id);
                int current = LevelData.AsIndex(two.levels[i].id);
                Assert.Less(previous, current, $"{two.levels[i - 1].id} then {two.levels[i].id}");
            }
        }

        [Test]
        public void ChapterTwoJobs_AreMultiShipAndSpeak()
        {
            ChapterData two = LoadChapter(ChapterTwoPath);
            for (int i = 0; i < two.levels.Length; i++)
            {
                PuzzleData puzzle = PuzzleOf(two.levels[i]);
                if (PuzzleJobContracts.TryGet(puzzle.id, out PuzzleJobContract contract))
                    Assert.AreEqual(contract.Ships, puzzle.ships.Length, puzzle.id);
                else
                    TestContext.WriteLine($"{puzzle.id}: no contract (hand-authored)");

                Assert.IsFalse(puzzle.noIntro, puzzle.id);
                Assert.IsFalse(puzzle.noFail, puzzle.id);
                Assert.IsFalse(puzzle.noGrade, puzzle.id);
            }
        }

        [Test]
        public void ChapterTwoJobs_KeepEveryShipClearOfFramesAndTheSun()
        {
            ChapterData two = LoadChapter(ChapterTwoPath);
            for (int i = 0; i < two.levels.Length; i++)
            {
                PuzzleData puzzle = PuzzleOf(two.levels[i]);
                for (int s = 0; s < puzzle.ships.Length; s++)
                {
                    PuzzleShip ship = puzzle.ships[s];
                    string label = $"{puzzle.id} ship {s}";
                    AssertOutsideFrames(puzzle, ship.origin, label + " origin");
                    AssertOutsideFrames(puzzle, ship.destination, label + " destination");
                    AssertNotOnTheSun(puzzle, ship, label);
                }
            }
        }

        private static ChapterData LoadChapter(string path)
        {
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
            Assert.IsNotNull(chapter, path);
            return chapter;
        }

        private static PuzzleData PuzzleOf(LevelData level)
        {
            PuzzleData puzzle = level as PuzzleData;
            Assert.IsNotNull(puzzle, $"{level.id} is not a PuzzleData");
            return puzzle;
        }

        private static void AssertOutsideFrames(PuzzleData puzzle, Vector3 point, string label)
        {
            for (int f = 0; f < puzzle.frames.Length; f++)
            {
                PuzzleFrame frame = puzzle.frames[f];
                Vector3 delta = point - frame.centre;
                Vector3 half = frame.BoxSize * 0.5f;
                bool inside = Mathf.Abs(delta.x) <= half.x
                    && Mathf.Abs(delta.y) <= half.y
                    && Mathf.Abs(delta.z) <= half.z;
                Assert.IsFalse(inside, $"{label} sits inside frame {f}");
            }
        }

        private static void AssertNotOnTheSun(PuzzleData puzzle, PuzzleShip ship, string label)
        {
            if (puzzle.sources == null)
                return;

            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                PuzzleSource sun = puzzle.sources[i];
                if (!sun.centreOnly)
                    continue;

                float radius = sun.scale * 0.5f;
                Assert.GreaterOrEqual(
                    Vector3.Distance(ship.origin, sun.home), radius, $"{label} starts on the sun");
                Assert.GreaterOrEqual(
                    Vector3.Distance(ship.destination, sun.home), radius, $"{label} ends on the sun");
            }
        }
    }
}
