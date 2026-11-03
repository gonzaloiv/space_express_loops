namespace DigitalLove.Game.SpaceShaping
{
    public static class PuzzleReward
    {
        public static string Describe(int grade) =>
            grade > 0 ? new string('★', grade) + " " + Tier(grade) : "-";

        public static string Tier(int grade)
        {
            if (grade >= 3)
                return "PERFECT";

            if (grade == 2)
                return "CLEAN";

            return grade == 1 ? "DELIVERED" : "-";
        }
    }
}
