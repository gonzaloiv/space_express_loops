using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

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

        public void Apply(Vector3[] positions, float[] scales, SourceSpec[] specs) =>
            Zone.ApplyTo(positions, scales, specs);
    }
}
