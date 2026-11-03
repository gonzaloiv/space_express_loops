namespace DigitalLove.Game.SpaceShaping
{
    public class PuzzleLabNextButton : PuzzleLabResetButton
    {
        protected override void Activate() => panel.NextPuzzle();
    }
}
