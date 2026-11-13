using DigitalLove.Casual.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class FleetJobsTests
    {
        private const string ChapterFolder = "Assets/_Game/Resources/Chapters/02";
        private const string SelectorPath = "Assets/_Game/Resources/Chapters/LevelSelector.asset";

        private static readonly string[] JobIds = { "02_01", "02_02", "02_03", "02_04" };
        private static readonly int[] ShipCounts = { 2, 2, 2, 3 };

        [Test]
        public void ChapterTwo_IsTheFourJobsAfterChapterOne()
        {
            LevelSelector selector = AssetDatabase.LoadAssetAtPath<LevelSelector>(SelectorPath);
            Assert.IsNotNull(selector, SelectorPath);

            for (int i = 0; i < JobIds.Length; i++)
            {
                LevelData level = selector.GetLevelByGlobalIndex<LevelData>(7 + i);
                Assert.IsNotNull(level, JobIds[i]);
                Assert.AreEqual(JobIds[i], level.id);
            }
        }

        [Test]
        public void ChapterTwoJobs_AreMultiShipAndSpeak()
        {
            for (int i = 0; i < JobIds.Length; i++)
            {
                PuzzleData puzzle = Load(JobIds[i]);
                Assert.AreEqual(ShipCounts[i], puzzle.ships.Length, JobIds[i]);
                Assert.IsFalse(puzzle.noIntro, JobIds[i]);
                Assert.IsFalse(puzzle.noFail, JobIds[i]);
                Assert.IsFalse(puzzle.noGrade, JobIds[i]);
            }
        }

        [Test]
        public void ChapterTwoJobs_KeepEveryShipClearOfFramesAndTheSun()
        {
            for (int i = 0; i < JobIds.Length; i++)
            {
                PuzzleData puzzle = Load(JobIds[i]);
                for (int s = 0; s < puzzle.ships.Length; s++)
                {
                    PuzzleShip ship = puzzle.ships[s];
                    string label = $"{JobIds[i]} ship {s}";
                    AssertOutsideFrames(puzzle, ship.origin, label + " origin");
                    AssertOutsideFrames(puzzle, ship.destination, label + " destination");
                    AssertNotOnTheSun(puzzle, ship, label);
                }
            }
        }

        private static PuzzleData Load(string id)
        {
            string path = $"{ChapterFolder}/Job_{id}_Puzzle.asset";
            PuzzleData puzzle = AssetDatabase.LoadAssetAtPath<PuzzleData>(path);
            Assert.IsNotNull(puzzle, path);
            Assert.AreEqual(id, puzzle.id, path);
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
