namespace DigitalLove.Game.SpaceShaping.Cube
{
    // What a hand near the cube can do from where it is: nothing (none), reach it (near: the grab
    // volume's margin, a two-hand scale spot), scale it (inside the scaled space), or pick it up and
    // move it (inside a movable cube's frame).
    public enum CubeHover
    {
        None,
        Near,
        Scale,
        Move
    }
}
