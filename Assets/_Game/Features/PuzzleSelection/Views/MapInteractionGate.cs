namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapInteractionGate
    {
        private readonly MapCube cube;
        private bool enabled = true;

        public MapInteractionGate(MapCube cube) => this.cube = cube;

        public bool IsEnabled => enabled;

        public void Reset() => enabled = true;

        public void Set(bool value, bool canEnter)
        {
            enabled = value;
            cube.SetInteractive(value, canEnter);
        }
    }
}
