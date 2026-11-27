namespace DigitalLove.Game.SpaceShaping.Creator
{
    // One shape generation for a seed: the constructor builds its field from this, and the fan-out
    // aim re-samples the same shape to keep a moved destination on the field's arms.
    public static class PuzzleConstructShape
    {
        public static PuzzleFieldPlanet[] Generate(PuzzleConstructRequest request, int seedValue)
        {
            SectorShapeData shape =
                PuzzleFieldBuilder.Build(request.Seed.WithSeed(seedValue), request.Field);
            return PuzzleFieldGenerator.Generate(shape, seedValue);
        }
    }
}
