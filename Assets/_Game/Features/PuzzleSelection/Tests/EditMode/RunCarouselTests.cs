using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class RunCarouselTests
    {
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
                Object.DestroyImmediate(created[i]);

            created.Clear();
        }

        [Test]
        public void FromLevels_MarksSolvedCurrentAndLocked()
        {
            PuzzleData[] levels = Levels(3);
            RunCarousel carousel = RunCarousel.FromLevels(
                levels, RunProgress.FromSolved(3, 1, new[] { true, false, false }));

            Assert.AreEqual(1, carousel.FocusedIndex);
            Assert.AreEqual(JobState.Solved, carousel[0].State);
            Assert.AreEqual(JobState.Current, carousel[1].State);
            Assert.AreEqual(JobState.Locked, carousel[2].State);
            Assert.IsTrue(carousel.CanEnter);
        }

        [Test]
        public void FromLevels_UsesSnapshotStars()
        {
            PuzzleData[] levels = Levels(3);
            RunProgress progress = RunProgress.FromSolved(
                3, 1, new[] { true, false, false }, new[] { 3, 0, 0 });
            RunCarousel carousel = RunCarousel.FromLevels(levels, progress);

            Assert.AreEqual(3, carousel[0].Stars);
            Assert.AreEqual(0, carousel[1].Stars);
        }

        [Test]
        public void Focus_RejectsLockedJobs()
        {
            RunCarousel carousel = RunCarousel.FromLevels(
                Levels(3), RunProgress.FromSolved(3, 1, new[] { true, false, false }));

            Assert.IsFalse(carousel.Focus(2));
            Assert.AreEqual(1, carousel.FocusedIndex);
            Assert.AreEqual(JobState.Current, carousel.Focused.State);
            Assert.IsTrue(carousel.CanEnter);
        }

        [Test]
        public void Focus_ClampsAndIgnoresTheCurrentIndex()
        {
            RunCarousel carousel = RunCarousel.FromLevels(
                Levels(3), RunProgress.FromSolved(3, 1, new[] { true, false, false }));

            Assert.IsFalse(carousel.Focus(1));
            Assert.IsFalse(carousel.Focus(99));
            Assert.AreEqual(1, carousel.FocusedIndex);
            Assert.IsTrue(carousel.Focus(0));
        }

        [Test]
        public void Move_ClampsToTheRun()
        {
            RunCarousel carousel = RunCarousel.FromLevels(
                Levels(3), RunProgress.FromSolved(3, 2, new[] { true, true, false }));

            Assert.IsTrue(carousel.Move(-10));
            Assert.AreEqual(0, carousel.FocusedIndex);
            Assert.IsTrue(carousel.Move(10));
            Assert.AreEqual(2, carousel.FocusedIndex);
            Assert.IsFalse(carousel.Move(1));
        }

        [Test]
        public void EmptyRun_CannotEnter()
        {
            RunCarousel carousel = RunCarousel.FromLevels(
                new PuzzleData[0], RunProgress.FromSolved(0, 0, null));

            Assert.AreEqual(0, carousel.Count);
            Assert.IsFalse(carousel.CanEnter);
            Assert.IsFalse(carousel.Focus(0));
            Assert.IsFalse(carousel.Move(1));
        }

        [Test]
        public void CurrentMissingPuzzle_CannotEnter()
        {
            PuzzleData[] levels = { Puzzle("01_01"), null };
            RunCarousel carousel = RunCarousel.FromLevels(
                levels, RunProgress.FromSolved(2, 1, new[] { false, false }));

            Assert.IsFalse(carousel.CanEnter);
        }

        private PuzzleData[] Levels(int count)
        {
            PuzzleData[] levels = new PuzzleData[count];
            for (int i = 0; i < count; i++)
                levels[i] = Puzzle($"01_{i + 1:00}");

            return levels;
        }

        private PuzzleData Puzzle(string id)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.id = id;
            created.Add(puzzle);
            return puzzle;
        }
    }
}
