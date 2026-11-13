using System;
using System.Collections.Generic;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using Newtonsoft.Json;

namespace DigitalLove.Game.Persistence
{
    public static class RunProgressCookies
    {
        private const string EndingSeenCookieId = "run_ending_seen";
        private const string ExitPromptCookieId = "exit_prompt_after_fail";
        private const string ChapterOpenedCookiePrefix = "chapter_opened_";

        public static RunProgress Build(LevelSelector selector, PlayerData playerData)
        {
            int count = selector.TotalLevelsCount;
            bool[] solved = new bool[count];
            int[] stars = new int[count];
            List<LevelCompleteCookie> cookies = playerData.GetLevelCompleteCookies();
            for (int i = 0; i < count; i++)
            {
                LevelData level = selector.GetLevelByGlobalIndex<LevelData>(i);
                if (level == null || string.IsNullOrEmpty(level.id))
                    continue;

                LevelCompleteCookie cookie = cookies.GetByLevelId(level.id);
                if (cookie == null)
                    continue;

                solved[i] = true;
                stars[i] = cookie.Score;
            }

            return RunProgress.FromSolved(
                count, CurrentIndex(selector, cookies), solved, stars,
                HighestOpenedIndex(selector, playerData));
        }

        public static JobOpenResult OpenNext(
            PlayerData playerData, LevelSelector selector, PuzzleData puzzle)
        {
            int index = selector.GetGlobalIndex(puzzle);
            int nextIndex = index + 1;
            if (index < 0 || nextIndex >= selector.TotalLevelsCount)
                return JobOpenResult.None;

            LevelData next = selector.GetLevelByGlobalIndex<LevelData>(nextIndex);
            if (next == null || playerData.HasCookie(new LevelCompleteCookie(next.id).id))
                return JobOpenResult.None;

            RunProgress before = Build(selector, playerData);
            if (nextIndex <= before.CurrentIndex)
                return JobOpenResult.None;

            bool firstOpen = playerData.AddCookie(new JobOpenedCookie(next.id));
            bool showExitPrompt = firstOpen && !playerData.HasCookie(ExitPromptCookieId);
            if (showExitPrompt)
                playerData.AddCookie(new Cookie(ExitPromptCookieId));

            RunProgress progress = Build(selector, playerData);
            int signpostIndex = progress.IsOpenedUnsolved(nextIndex) ? nextIndex : -1;
            return new JobOpenResult(firstOpen, signpostIndex, showExitPrompt);
        }

        // True only the first time: a chapter's opener plays once, never on a replay.
        public static bool OpenChapter(PlayerData playerData, string chapterId) =>
            playerData.AddCookie(new Cookie(ChapterOpenedCookiePrefix + chapterId));

        public static bool HasSeenEnding(PlayerData playerData, int jobCount)
        {
            Cookie cookie = playerData.GetCookieById(EndingSeenCookieId);
            if (cookie == null || string.IsNullOrEmpty(cookie.metadata))
                return false;

            try
            {
                EndingSeenMetadata metadata =
                    JsonConvert.DeserializeObject<EndingSeenMetadata>(cookie.metadata);
                return metadata != null && metadata.jobCount >= jobCount;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static void MarkEndingSeen(PlayerData playerData, int jobCount)
        {
            Cookie cookie = playerData.GetOrCreateCookie(EndingSeenCookieId);
            cookie.SetMetadata(JsonConvert.SerializeObject(
                new EndingSeenMetadata { jobCount = jobCount }));
        }

        public static bool RecordSolve(PlayerData playerData, string levelId, int grade)
        {
            Cookie stored = playerData.GetCookieById(new LevelCompleteCookie(levelId).id);
            if (stored == null)
            {
                playerData.cookies.Add(CompletedCookie(levelId, grade));
                return true;
            }

            if (new LevelCompleteCookie(stored).Score >= grade)
                return false;

            stored.SetMetadata(Metadata(grade));
            return true;
        }

        private static Cookie CompletedCookie(string levelId, int grade) =>
            new LevelCompleteCookie(levelId).SetMetadata(Metadata(grade));

        private static string Metadata(int grade) =>
            new LevelCompleteCookieMetadata { hasBeenCompleted = true, score = grade }
                .Serialize<LevelCompleteCookieMetadata>();

        private static int CurrentIndex(
            LevelSelector selector, List<LevelCompleteCookie> cookies)
        {
            PuzzleData current = selector.GetFollowingLevel<PuzzleData>(cookies);
            return current != null ? selector.GetGlobalIndex(current) : 0;
        }

        private static int HighestOpenedIndex(LevelSelector selector, PlayerData playerData)
        {
            int highest = -1;
            int count = selector.TotalLevelsCount;
            for (int i = 0; i < count; i++)
            {
                LevelData level = selector.GetLevelByGlobalIndex<LevelData>(i);
                if (level == null || string.IsNullOrEmpty(level.id))
                    continue;

                if (playerData.HasCookie(new JobOpenedCookie(level.id).id))
                    highest = i;
            }

            return highest;
        }

        [Serializable]
        private class EndingSeenMetadata
        {
            public int jobCount;
        }
    }
}
