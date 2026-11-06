using System.Collections.Generic;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.Persistence.Tests
{
    public class RunProgressCookiesTests
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
        public void Build_UsesCompletedCookiesAndScores()
        {
            LevelSelector selector = Selector(Puzzle("01_01"), Puzzle("01_02"), Puzzle("01_03"));
            PlayerData playerData = new();
            AddComplete(playerData, "01_01", 3);

            RunProgress progress = RunProgressCookies.Build(selector, playerData);

            Assert.IsTrue(progress.IsSolved(0));
            Assert.AreEqual(3, progress.Stars(0));
            Assert.IsFalse(progress.IsSolved(1));
            Assert.AreEqual(1, progress.CurrentIndex);
            Assert.IsFalse(progress.IsFresh);
        }

        [Test]
        public void Build_FreshPlayerStartsAtTheFirstJob()
        {
            LevelSelector selector = Selector(Puzzle("01_01"), Puzzle("01_02"));

            RunProgress progress = RunProgressCookies.Build(selector, new PlayerData());

            Assert.IsTrue(progress.IsFresh);
            Assert.AreEqual(0, progress.CurrentIndex);
            Assert.IsFalse(progress.IsSolved(0));
        }

        [Test]
        public void Build_AllCompleteMarksEveryJobSolved()
        {
            PuzzleData[] levels = { Puzzle("01_01"), Puzzle("01_02") };
            LevelSelector selector = Selector(levels);
            PlayerData playerData = new();
            AddComplete(playerData, "01_01", 2);
            AddComplete(playerData, "01_02", 1);

            RunProgress progress = RunProgressCookies.Build(selector, playerData);
            JobEntry[] entries = RunJobs.FromLevels(levels, progress);

            Assert.AreEqual(JobState.Solved, entries[0].State);
            Assert.AreEqual(JobState.Solved, entries[1].State);
        }

        [Test]
        public void RecordSolve_AddsCookieWithGrade()
        {
            PlayerData playerData = new();

            bool changed = RunProgressCookies.RecordSolve(playerData, "01_01", 2);

            Assert.IsTrue(changed);
            Assert.IsNotNull(playerData.GetCookieById("level_complete_01_01"));
            Assert.AreEqual(2, Score(playerData, "01_01"));
        }

        [Test]
        public void RecordSolve_KeepsTheBestGrade()
        {
            PlayerData playerData = new();
            AddComplete(playerData, "01_03", 2);

            Assert.IsFalse(RunProgressCookies.RecordSolve(playerData, "01_03", 1));
            Assert.IsFalse(RunProgressCookies.RecordSolve(playerData, "01_03", 2));
            Assert.AreEqual(2, Score(playerData, "01_03"));
            Assert.IsTrue(RunProgressCookies.RecordSolve(playerData, "01_03", 3));
            Assert.AreEqual(3, Score(playerData, "01_03"));
            Assert.AreEqual(1, playerData.cookies.Count);
        }

        [Test]
        public void RecordSolve_UngradedSolveStillMarksTheJob()
        {
            LevelSelector selector = Selector(Puzzle("01_01"));
            PlayerData playerData = new();

            RunProgressCookies.RecordSolve(playerData, "01_01", 0);
            RunProgress progress = RunProgressCookies.Build(selector, playerData);

            Assert.IsTrue(progress.IsSolved(0));
            Assert.AreEqual(0, progress.Stars(0));
        }

        private LevelSelector Selector(params PuzzleData[] levels)
        {
            ChapterData chapter = ScriptableObject.CreateInstance<ChapterData>();
            chapter.id = "01";
            chapter.levels = levels;
            created.Add(chapter);

            LevelSelector selector = ScriptableObject.CreateInstance<LevelSelector>();
            created.Add(selector);
            SerializedObject serialized = new(selector);
            SerializedProperty chapters = serialized.FindProperty("chapters");
            chapters.arraySize = 1;
            chapters.GetArrayElementAtIndex(0).objectReferenceValue = chapter;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return selector;
        }

        private PuzzleData Puzzle(string id)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.id = id;
            created.Add(puzzle);
            return puzzle;
        }

        private static void AddComplete(PlayerData playerData, string levelId, int score)
        {
            Cookie cookie = new LevelCompleteCookie(levelId);
            LevelCompleteCookieMetadata metadata = new() { hasBeenCompleted = true, score = score };
            cookie.SetMetadata(metadata.Serialize<LevelCompleteCookieMetadata>());
            playerData.cookies.Add(cookie);
        }

        private static int Score(PlayerData playerData, string levelId) =>
            playerData.GetLevelCompleteCookies().GetByLevelId(levelId).Score;
    }
}
