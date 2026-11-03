using DigitalLove.Game.SpaceShaping.Cube;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceScaleCommand
    {
        public SpaceScaleCommand(SpaceZone zone, float entropyCost)
        {
            Zone = zone;
            EntropyCost = entropyCost;
        }

        public SpaceZone Zone { get; }

        public float EntropyCost { get; }
    }
}
