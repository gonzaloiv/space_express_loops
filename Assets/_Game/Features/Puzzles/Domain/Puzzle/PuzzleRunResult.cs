namespace DigitalLove.Game.SpaceShaping
{
    public readonly struct PuzzleRunResult
    {
        public PuzzleRunResult(bool solved, int grade)
        {
            Solved = solved;
            Grade = grade;
        }

        public bool Solved { get; }
        public int Grade { get; }
    }
}
