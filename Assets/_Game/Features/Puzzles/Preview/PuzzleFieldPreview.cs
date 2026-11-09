namespace DigitalLove.Game.SpaceShaping
{
    // Shows a still preview of the field for the inspector's Preview field command.
    public static class PuzzleFieldPreview
    {
        public static void Show(SpaceShapingSetup setup, SpaceShapingView view)
        {
            PuzzleRun preview = new(setup);
            TrajectoryResult[] results = preview.SimulatePreview();
            view.Render(preview.Session);
            view.ShowBaseline(preview.Session);
            view.ShowTrajectories(preview.Session, results, false);
        }
    }
}
