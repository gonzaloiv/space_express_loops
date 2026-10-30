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
                specs[i] = new SourceSpec
                {
                    Home = sources[i].home,
                    InitialScale = scale,
                    MinScale = scale * settings.MinScaleFactor,
                    MaxScale = scale * settings.MaxScaleFactor,
                    Name = sources[i].name
                };
            }

            return specs;
        }
    }
}
