using DigitalLove.DataAccess;
using DigitalLove.Global;

namespace DigitalLove.Game.Persistence
{
    public static class PlayerDataHighScoreExtensions
    {
        public static int GetHighScoreMoney(this PlayerData playerData, StringValue cookieKey)
        {
            if (!playerData.HasCookie(cookieKey))
                return 0;

            Cookie cookie = playerData.GetCookieById(cookieKey.value);
            return int.TryParse(cookie?.metadata, out int score) ? score : 0;
        }

        public static bool TrySaveHighScoreMoney(this PlayerData playerData, StringValue cookieKey, int totalMoney)
        {
            if (totalMoney <= playerData.GetHighScoreMoney(cookieKey))
                return false;

            playerData.SetHighScoreMoney(cookieKey, totalMoney);
            return true;
        }

        public static void SetHighScoreMoney(this PlayerData playerData, StringValue cookieKey, int totalMoney)
        {
            Cookie cookie = playerData.GetOrCreateCookie(cookieKey.value);
            cookie.metadata = totalMoney.ToString();
        }
    }
}
