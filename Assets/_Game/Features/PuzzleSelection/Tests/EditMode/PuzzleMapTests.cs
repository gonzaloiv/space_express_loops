using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class PuzzleMapTests
    {
        private readonly List<PuzzleData> created = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
                Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        [Test]
        public void FromLevels_UsesLayoutAndProgress()
        {
            PuzzleMapLayout layout = PuzzleMapLayout.Galaxy(4);
            PuzzleMap map = PuzzleMap.FromLevels(
                Levels(4), RunProgress.FromSolved(4, 1, new[] { true, false, false, false }, new[] { 3, 0, 0, 0 }),
                layout);

            Assert.AreEqual(4, map.Count);
            Assert.AreEqual(1, map.SelectedIndex);
            Assert.AreEqual(JobState.Current, map.Selected.State);
            Assert.IsTrue(map.CanEnter);
            Assert.AreEqual(JobState.Solved, map[0].State);
            Assert.AreEqual(3, map[0].Stars);
            Assert.AreEqual(JobState.Locked, map[2].State);
            Assert.AreEqual(layout.At(2), map.Position(2));
            Assert.AreEqual(1, map.HighestCurrentIndex);
        }

        [Test]
        public void HighestCurrentIndex_PrefersTheOpenedJob()
        {
            PuzzleMap map = PuzzleMap.FromLevels(
                Levels(6),
                RunProgress.FromSolved(
                    6, 3, new[] { true, true, true, false, false, false },
                    new[] { 3, 3, 3, 0, 0, 0 }, highestOpened: 4),
                PuzzleMapLayout.Galaxy(6));

            Assert.AreEqual(JobState.Current, map[3].State);
            Assert.AreEqual(JobState.Current, map[4].State);
            Assert.AreEqual(JobState.Locked, map[5].State);
            Assert.AreEqual(4, map.HighestCurrentIndex);
        }

        [Test]
        public void HighestCurrentIndex_FallsBackToTheLastSolved()
        {
            PuzzleMap map = PuzzleMap.FromLevels(
                Levels(3),
                RunProgress.FromSolved(3, 2, new[] { true, true, true }, new[] { 1, 1, 1 }),
                PuzzleMapLayout.Galaxy(3));

            Assert.AreEqual(2, map.HighestCurrentIndex);
        }

        [Test]
        public void SelectNext_ClampsAndAllowsLockedPreview()
        {
            PuzzleMap map = PuzzleMap.FromLevels(
                Levels(4), RunProgress.FromSolved(4, 1, new[] { true, false, false, false }),
                PuzzleMapLayout.Galaxy(4));

            Assert.IsTrue(map.SelectNext(1));
            Assert.AreEqual(2, map.SelectedIndex);
            Assert.IsFalse(map.CanEnter);

            Assert.IsTrue(map.SelectNext(1));
            Assert.AreEqual(3, map.SelectedIndex);
            Assert.IsFalse(map.SelectNext(1));
            Assert.AreEqual(3, map.SelectedIndex);
            Assert.IsTrue(map.SelectNext(-1));
            Assert.AreEqual(2, map.SelectedIndex);
            Assert.IsTrue(map.SelectNext(10));
            Assert.AreEqual(3, map.SelectedIndex);
            Assert.IsTrue(map.SelectNext(-10));
            Assert.AreEqual(0, map.SelectedIndex);
            Assert.IsFalse(map.SelectNext(-1));
            Assert.AreEqual(0, map.SelectedIndex);

            Assert.IsFalse(map.SelectNext(0));
            Assert.IsTrue(map.Select(1));
            Assert.IsFalse(map.Select(1));
            Assert.IsFalse(map.Select(99));
        }

        [Test]
        public void GalaxyLayout_IsDeterministicAndUnique()
        {
            PuzzleMapLayout first = PuzzleMapLayout.Galaxy(8);
            PuzzleMapLayout second = PuzzleMapLayout.Galaxy(8);

            Assert.AreEqual(8, first.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first.At(i), second.At(i));
                for (int j = 0; j < i; j++)
                    Assert.Greater(Vector3.Distance(first.At(i), first.At(j)), 0.0001f);
            }

            Assert.AreEqual(0, PuzzleMapLayout.Galaxy(0).Count);
            Assert.AreEqual(Vector3.zero, PuzzleMapLayout.Galaxy(1).At(0));
        }

        [Test]
        public void NoGradeTutorial_ShowsNoStars()
        {
            PuzzleData[] levels = Levels(1);
            levels[0].noGrade = true;
            PuzzleMap map = PuzzleMap.FromLevels(
                levels, RunProgress.FromSolved(1, 0, new[] { true }, new[] { 0 }),
                PuzzleMapLayout.Galaxy(1));

            Assert.AreEqual(JobState.Solved, map[0].State);
            Assert.IsTrue(map[0].NoGrade);
            Assert.IsFalse(map[0].ShowsStars);
        }

        [Test]
        public void JobStillToComplete_ShowsEmptyStars_LockedShowsNone()
        {
            PuzzleData[] levels = Levels(2);
            PuzzleMap map = PuzzleMap.FromLevels(
                levels, RunProgress.FromSolved(2, 0, new[] { false, false }, new[] { 0, 0 }),
                PuzzleMapLayout.Galaxy(2));

            Assert.AreEqual(JobState.Current, map[0].State);
            Assert.IsTrue(map[0].ShowsStars);
            Assert.AreEqual(0, map[0].Stars);
            Assert.AreEqual(JobState.Locked, map[1].State);
            Assert.IsFalse(map[1].ShowsStars);
        }

        [Test]
        public void SolvedGradedJob_ShowsItsStars()
        {
            PuzzleData[] levels = Levels(1);
            PuzzleMap map = PuzzleMap.FromLevels(
                levels, RunProgress.FromSolved(1, 0, new[] { true }, new[] { 2 }),
                PuzzleMapLayout.Galaxy(1));

            Assert.IsFalse(map[0].NoGrade);
            Assert.IsTrue(map[0].ShowsStars);
            Assert.AreEqual(2, map[0].Stars);
        }

        [Test]
        public void IsComplete_RequiresEveryJob()
        {
            Assert.IsTrue(RunProgress.FromSolved(
                7, 0, new[] { true, true, true, true, true, true, true }).IsComplete);
            Assert.IsFalse(RunProgress.FromSolved(
                7, 0, new[] { true, true, true, true, true, true, false }).IsComplete);
            Assert.IsTrue(RunProgress.FromSolved(
                11, 0,
                new[]
                {
                    true, true, true, true, true, true,
                    true, true, true, true, true
                }).IsComplete);
            Assert.IsFalse(RunProgress.FromSolved(
                0, 0, System.Array.Empty<bool>()).IsComplete);
        }

        [Test]
        public void Totals_CountGradedJobsOnly()
        {
            PuzzleData[] levels = Levels(3);
            levels[0].noGrade = true;

            PuzzleMap map = PuzzleMap.FromLevels(
                levels,
                RunProgress.FromSolved(3, 0, new[] { true, true, true }, new[] { 0, 2, 3 }),
                PuzzleMapLayout.Galaxy(3));

            Assert.AreEqual(5, map.TotalStars);
            Assert.AreEqual(6, map.MaxStars);
        }

        private PuzzleData[] Levels(int count)
        {
            PuzzleData[] levels = new PuzzleData[count];
            for (int i = 0; i < count; i++)
            {
                PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
                puzzle.name = "Puzzle " + i;
                puzzle.sources = new[]
                {
                    new PuzzleSource { home = Vector3.zero, scale = 0.1f }
                };
                created.Add(puzzle);
                levels[i] = puzzle;
            }

            return levels;
        }
    }
}
