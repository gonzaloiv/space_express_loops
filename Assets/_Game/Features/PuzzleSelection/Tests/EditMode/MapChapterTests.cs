using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection.Tests
{
    public class MapChapterTests
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
        public void Systems_OneChapterIsThePlainGalaxy()
        {
            PuzzleMapLayout galaxy = PuzzleMapLayout.Galaxy(7);
            PuzzleMapLayout systems = PuzzleMapLayout.Systems(new[] { 7 });

            for (int i = 0; i < 7; i++)
                Assert.AreEqual(galaxy.At(i), systems.At(i));
        }

        [Test]
        public void Systems_KeepEachChapterInItsOwnCluster()
        {
            const float gap = 0.1f;
            PuzzleMapLayout layout = PuzzleMapLayout.Systems(new[] { 7, 4 }, 0.24f, gap);

            Assert.AreEqual(11, layout.Count);
            Assert.AreEqual(2, layout.SystemCount);
            for (int i = 0; i < 7; i++)
                AssertInside(layout, i, 0);
            for (int i = 7; i < 11; i++)
                AssertInside(layout, i, 1);

            float between = Vector3.Distance(layout.Centre(0), layout.Centre(1));
            Assert.GreaterOrEqual(between, layout.Radius(0) + layout.Radius(1) + gap - 0.0001f);
        }

        [Test]
        public void Systems_AreCentredOnTheMap()
        {
            PuzzleMapLayout layout = PuzzleMapLayout.Systems(new[] { 7, 4 });

            float left = layout.Centre(0).x - layout.Radius(0);
            float right = layout.Centre(1).x + layout.Radius(1);
            Assert.AreEqual(0f, left + right, 0.0001f);
        }

        [Test]
        public void Build_GroupsJobsByChapterAndMarksTheHop()
        {
            PuzzleData[] levels = Levels("01_01", "01_02", "01_03", "02_01", "02_02");
            PuzzleMap map = RunMap.Build(levels, RunProgress.FromSolved(5, 0, null));

            Assert.AreEqual(2, map.Chapters.Count);
            Assert.AreEqual("01", map.Chapters[0].Id);
            Assert.AreEqual(3, map.Chapters[0].Count);
            Assert.AreEqual("02", map.Chapters[1].Id);
            Assert.AreEqual(3, map.Chapters[1].Start);
            Assert.IsTrue(map.StartsChapter(3));
            Assert.IsFalse(map.StartsChapter(0));
            Assert.IsFalse(map.StartsChapter(4));
        }

        private static void AssertInside(PuzzleMapLayout layout, int index, int system)
        {
            float distance = Vector3.Distance(layout.At(index), layout.Centre(system));
            Assert.LessOrEqual(distance, layout.Radius(system) + 0.0001f, $"job {index}");
        }

        private PuzzleData[] Levels(params string[] ids)
        {
            PuzzleData[] levels = new PuzzleData[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
                puzzle.id = ids[i];
                created.Add(puzzle);
                levels[i] = puzzle;
            }

            return levels;
        }
    }
}
