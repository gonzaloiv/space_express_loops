using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A movable job's start cube. It sits at the brief's start spot, unless scaling the cube right
    // there already solves the job (moving it would be optional); then it takes the nearest candidate
    // spot where scaling alone fails and no endpoint is covered, so the player has to move it.
    public static class PuzzleStartFrame
    {
        private const int ScaleSamples = 25;

        public static Vector3 Place(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3 preferred, float size)
        {
            if (Accepts(puzzle, settings, preferred, size))
                return preferred;

            Vector3[] centres = PuzzleZoneCandidates.Centres(puzzle, size);
            System.Array.Sort(centres, (a, b) =>
                (a - preferred).sqrMagnitude.CompareTo((b - preferred).sqrMagnitude));
            for (int i = 0; i < centres.Length; i++)
            {
                if (Accepts(puzzle, settings, centres[i], size))
                    return centres[i];
            }

            return preferred;
        }

        // Any scale the player can reach (a grow-only job never shrinks) that lands every ship within
        // budget, with the cube unmoved.
        public static bool SolvesInPlace(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3 centre, float size)
        {
            if (puzzle.sources == null || puzzle.sources.Length == 0)
                return false;

            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, settings);
            float lowest = puzzle.MinScaleFactor(settings.MinScaleFactor);
            float[] scales = PuzzleZoneCandidates.Scales(settings, ScaleSamples);
            for (int i = 0; i < scales.Length; i++)
            {
                if (scales[i] < lowest)
                    continue;

                PuzzleZone zone = PuzzleZoneCandidates.Zone(centre, size, scales[i], settings.ZoneFalloff);
                SpaceShapingSession session = new(setup);
                session.ApplyZone(zone.ToSpaceZone());
                if (session.SimulateAll().AllArrived() && session.EntropySpent <= puzzle.entropyBudget)
                    return true;
            }

            return false;
        }

        private static bool Accepts(
            PuzzleData puzzle, PuzzleSimSettings settings, Vector3 centre, float size)
        {
            PuzzleZone resting = PuzzleZoneCandidates.Zone(centre, size, 1f, settings.ZoneFalloff);
            return !PuzzleZoneCandidates.ContainsEndpoint(resting, puzzle)
                && !SolvesInPlace(puzzle, settings, centre, size);
        }
    }
}
