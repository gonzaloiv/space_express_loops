using DigitalLove.DataAccess;

namespace DigitalLove.Game.Persistence
{
    public class JobOpenedCookie : Cookie
    {
        public const string Key = "job_opened_";

        public JobOpenedCookie(string levelId) : base(Key + levelId) { }
    }
}
