namespace DigitalLove.Game.GesturePrompts
{
    // One gesture identity shared by the prompt UI and the saved learned state.
    public enum GestureKind
    {
        Scale,
        Enter,
        Move,
        Exit,
        Spend,
        Stars,
        // Not a gesture: the tutorial's goal, pointing at the destination.
        Goal
    }
}