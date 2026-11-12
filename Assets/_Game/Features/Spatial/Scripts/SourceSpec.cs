using UnityEngine;

namespace DigitalLove.Game.Spatial
{
    public struct SourceSpec
    {
        public Vector3 Home;
        public float InitialScale;
        public float MinScale;
        public float MaxScale;
        public string Name;
        public bool CentreOnly;
        public bool FixedMass;

        public SourceSpec Scaled(Vector3 centre, float factor) => new()
        {
            Home = centre + (Home - centre) * factor,
            InitialScale = InitialScale * factor,
            MinScale = MinScale * factor,
            MaxScale = MaxScale * factor,
            Name = Name,
            CentreOnly = CentreOnly,
            FixedMass = FixedMass
        };
    }
}
