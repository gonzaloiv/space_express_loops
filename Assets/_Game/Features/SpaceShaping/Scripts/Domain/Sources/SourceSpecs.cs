namespace DigitalLove.Game.SpaceShaping
{
    public static class SourceSpecs
    {
        public static SourceSpec[] From(PuzzleSource[] sources, SpaceShapingSettings settings)
        {
            SourceSpec[] specs = new SourceSpec[sources.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                float scale = sources[i].scale;
                specs[i] = new SourceSpec
                {
                    Home = sources[i].home,
                    InitialScale = scale,
                    MoveRange = scale,
                    MinScale = scale * settings.minScaleFactor,
                    MaxScale = scale * settings.maxScaleFactor
                };
            }

            return specs;
        }
    }
}
