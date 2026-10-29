namespace DigitalLove.Game.SpaceShaping
{
    public static class PuzzleGrade
    {
        public const float BudgetFactor = 3f;

        private const float ThreeStarUse = 1f / 3f;
        private const float TwoStarUse = 2f / 3f;

        public static int Stars(float spent, float budget)
        {
            float fraction = budget > 0f ? spent / budget : 0f;
            if (fraction <= ThreeStarUse)
                return 3;

            return fraction <= TwoStarUse ? 2 : 1;
        }
    }
}
