using DigitalLove.Game.SpaceShaping.Cube;

namespace DigitalLove.Game.SpaceShaping
{
    public static class SourceSpecs
    {
        public static SourceSpec[] From(PuzzleSource[] sources, PuzzleSimSettings settings)
        {
            SourceSpec[] specs = new SourceSpec[sources.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                float scale = sources[i].scale;
                bool fixedMass = sources[i].fixedMass;
                specs[i] = new SourceSpec
                {
                    Home = sources[i].home,
                    InitialScale = scale,
                    MinScale = fixedMass ? scale : scale * settings.MinScaleFactor,
                    MaxScale = fixedMass ? scale : scale * settings.MaxScaleFactor,
                    Name = sources[i].name,
                    CentreOnly = sources[i].centreOnly,
                    FixedMass = fixedMass
                };
            }

            return specs;
        }
    }
}
