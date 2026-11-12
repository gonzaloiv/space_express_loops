using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal static class PuzzleSunSources
    {
        // The sun is a source at the field's centre, fixed or scalable as the seed says. It stays out of
        // the cube range so a planet-sized cube still reads as the player's tool, while the run choice
        // keeps the star's band clear.
        public static PuzzleSource[] WithSun(
            PuzzleSource[] sources, float sunRadius, bool sunScalable)
        {
            if (sunRadius <= 0f)
                return sources;

            PuzzleSource[] withSun = new PuzzleSource[sources.Length + 1];
            withSun[0] = new PuzzleSource
            {
                home = Vector3.zero,
                scale = sunRadius * 2f,
                name = SunName(sources.Length),
                fixedMass = !sunScalable,
                centreOnly = true
            };

            for (int i = 0; i < sources.Length; i++)
                withSun[i + 1] = sources[i];

            return withSun;
        }

        private static string SunName(int planetCount)
        {
            PlanetNames names = PlanetNames.Instance;
            return names != null ? names.NameAt(planetCount) : string.Empty;
        }
    }
}
