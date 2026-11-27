using System.Collections.Generic;
using DigitalLove.Casual.Levels;
using NUnit.Framework;
using UnityEditor;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    // Bakes no longer rewrite Chapters/<nn>.asset, so these checks keep the chapter order and the
    // contract list in step. They load assets only; nothing here bakes.
    public class PuzzleJobChapterTests
    {
        private const string ChaptersFolder = "Assets/_Game/Resources/Chapters";

        [Test]
        public void EnabledContracts_AreListedInTheirChapter()
        {
            List<string> failures = new List<string>();
            for (int i = 0; i < PuzzleJobContracts.Enabled.Count; i++)
            {
                PuzzleJobContract contract = PuzzleJobContracts.Enabled[i];
                ChapterData chapter =
                    Load(PuzzleJobContracts.ChapterOf(contract.Id), failures);
                if (chapter != null && IndexOf(chapter, contract.Id) < 0)
                    failures.Add($"{contract.Id}: not listed in chapter {chapter.id}");
            }

            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }

        [Test]
        public void ListedJobs_HaveAContractOrAreAuthoredByHand()
        {
            List<string> failures = new List<string>();
            List<ChapterData> chapters = LoadChapters(failures);
            for (int i = 0; i < chapters.Count; i++)
                CheckListed(chapters[i], failures);

            Assert.AreEqual(0, failures.Count, string.Join("\n", failures));
        }

        private static void CheckListed(ChapterData chapter, List<string> failures)
        {
            for (int i = 0; i < chapter.levels.Length; i++)
            {
                LevelData level = chapter.levels[i];
                if (level == null)
                {
                    failures.Add($"chapter {chapter.id} slot {i} is empty");
                    continue;
                }

                if (!PuzzleJobContracts.Has(level.id)
                    && !PuzzleJobContracts.IsAuthoredByHand(level.id))
                    failures.Add($"{level.id}: listed in chapter {chapter.id} with no contract");
            }
        }

        private static List<ChapterData> LoadChapters(List<string> failures)
        {
            string[] guids = AssetDatabase.FindAssets("t:ChapterData", new[] { ChaptersFolder });
            List<string> paths = new List<string>();
            for (int i = 0; i < guids.Length; i++)
                paths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
            paths.Sort();

            List<ChapterData> chapters = new List<ChapterData>();
            for (int i = 0; i < paths.Count; i++)
            {
                ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(paths[i]);
                if (chapter == null)
                    failures.Add($"missing chapter at {paths[i]}");
                else
                    chapters.Add(chapter);
            }

            return chapters;
        }

        private static ChapterData Load(string id, List<string> failures)
        {
            string path = $"{ChaptersFolder}/{id}.asset";
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
            if (chapter == null)
                failures.Add($"missing chapter at {path}");
            return chapter;
        }

        private static int IndexOf(ChapterData chapter, string id)
        {
            for (int i = 0; i < chapter.levels.Length; i++)
            {
                if (chapter.levels[i] != null && chapter.levels[i].id == id)
                    return i;
            }

            return -1;
        }
    }
}
