namespace DigitalLove.Game.SpaceShaping
{
    public static class PuzzleGrade
    {
        public const float BudgetFactor = 3f;

        public const int MaxStars = 3;

        public const float ThreeStarUse = 0.5f;
        public const float TwoStarUse = 2f / 3f;

        public static int Stars(float spent, float budget)
        {
            float fraction = budget > 0f ? spent / budget : 0f;
            if (fraction <= ThreeStarUse)
                return MaxStars;

            return fraction <= TwoStarUse ? 2 : 1;
        }
    }
}
