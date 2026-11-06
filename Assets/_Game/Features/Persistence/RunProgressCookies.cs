using System.Collections.Generic;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.Persistence
{
    public static class RunProgressCookies
    {
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

            return RunProgress.FromSolved(count, CurrentIndex(selector, cookies), solved, stars);
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
    }
}
