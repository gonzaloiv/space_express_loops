namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The cheap half of the shipped check: replay the stored zones through the bake's verifier. Names
    // every job whose answer stopped landing or changed role after a settings change.
    public static class PuzzleAnswerReplay
    {
        public static bool Lands(
            PuzzleData puzzle, PuzzleAnswerAsset answer, PuzzleSimSettings settings, out string problem)
        {
            problem = string.Empty;
            PuzzleVerification check = PuzzleVerifier.Verify(puzzle, answer.zones, settings);
            if (!check.Solved)
            {
                problem = "the stored answer no longer lands";
                return false;
            }

            if (!check.StartFails)
            {
                problem = "the stored answer already lands from the start";
                return false;
            }

            return SameKind(check, answer, out problem);
        }

        private static bool SameKind(
            PuzzleVerification check, PuzzleAnswerAsset answer, out string problem)
        {
            problem = string.Empty;
            if (check.Kind == answer.kind)
                return true;

            problem = $"the stored answer now classifies as {check.Kind}, was {answer.kind}";
            return false;
        }
    }
}
