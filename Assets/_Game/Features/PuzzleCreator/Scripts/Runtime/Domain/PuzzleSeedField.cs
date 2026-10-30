using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A seeded field without a solution search: planets from the shape, then PuzzleFieldBake.Apply to
    // pick the run and lay out the sources. Null when no run can be placed.
    internal static class PuzzleSeedField
    {
        public static PuzzleData Build(PuzzleSeed seed, SpaceShapingSettings settings, int seedValue)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            PuzzleSeed probe = Object.Instantiate(seed);
            probe.seed = seedValue;
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(probe, settings);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            bool baked = PuzzleFieldBake.Apply(brief, field, planets, sim);
            Object.DestroyImmediate(planets);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(probe);
            if (baked)
                return field;

            Object.DestroyImmediate(field);
            return null;
        }
    }
}
