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

        public void Apply(Vector3[] positions, float[] scales, SourceSpec[] specs)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                float factor = Mathf.Lerp(1f, Zone.Scale, Zone.InsideFactor(positions[i]));
                positions[i] = Zone.Centre + (positions[i] - Zone.Centre) * factor;
                scales[i] *= factor;
                specs[i] = Scaled(specs[i], Zone.Centre, factor);
            }
        }

        private static SourceSpec Scaled(SourceSpec source, Vector3 centre, float factor) => new()
        {
            Home = centre + (source.Home - centre) * factor,
            InitialScale = source.InitialScale * factor,
            MoveRange = source.MoveRange * factor,
            MinScale = source.MinScale * factor,
            MaxScale = source.MaxScale * factor
        };
    }
}
