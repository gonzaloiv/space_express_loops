using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Everything the constructor and its verdict read, plain except the contract/brief data types.
    public sealed class PuzzleConstructRequest
    {
        public PuzzleJobContract Contract;
        public PuzzleBrief Brief;
        public PuzzleSeedFields Seed;
        public PuzzleFieldSettings Field;
        public PuzzleSimSettings Sim;
        public IPuzzleLesson Lesson;
        public int Attempt;

        public PuzzleConstructRequest WithAttempt(int attempt) => new()
        {
            Contract = Contract,
            Brief = Brief,
            Seed = Seed,
            Field = Field,
            Sim = Sim,
            Lesson = Lesson,
            Attempt = attempt
        };
    }
}
