using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSeedScanner
    {
        public static bool IsBetter(PuzzleSeedScanResult candidate, PuzzleSeedScanResult best) =>
            candidate.Found && (!best.Found || candidate.Routes > best.Routes);

        public static PuzzleSeedScanResult Find(
            PuzzleSeed seed, SpaceShapingSettings settings, int attempts)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            PuzzleSeedScanResult best = new() { Seed = seed.seed };
            for (int i = 0; i < attempts; i++)
            {
                PuzzleSeedScanResult candidate = Probe(seed, settings, sim, brief, seed.seed + i);
                if (!candidate.Found)
                {
                    if (!best.Found)
                        best.Status = candidate.Status;
                    continue;
                }

                Keep(ref best, candidate);

                // Multi-ship bakes are much heavier and have no route ranking, so the first one
                // that bakes is the answer.
                if (brief.IsMulti)
                    break;
            }

            return best;
        }

        private static PuzzleSeedScanResult Probe(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleSimSettings sim, PuzzleBrief brief,
            int seedValue)
        {
            PuzzlePlanetListAsset planets = Planets(seed, settings, seedValue);
            if (brief.IsMulti)
                return ProbeMulti(planets, sim, brief, seedValue);

            PuzzleData baked = PuzzleFieldBake.Bake(
                planets, sim, brief, out PuzzleBakeStatus status, out PuzzleZone[] solution);
            Object.DestroyImmediate(planets);
            return new PuzzleSeedScanResult
            {
                Found = baked != null,
                Seed = seedValue,
                Status = status,
                Puzzle = baked,
                Solution = solution,
                Routes = baked != null ? baked.routes : 0
            };
        }

        private static PuzzleSeedScanResult ProbeMulti(
            PuzzlePlanetListAsset planets, PuzzleSimSettings sim, PuzzleBrief brief, int seedValue)
        {
            PuzzleMultiShipResult result = PuzzleMultiShipBake.Bake(planets, sim, brief);
            Object.DestroyImmediate(planets);
            PuzzleData baked = result.Status == PuzzleBakeStatus.Baked ? result.Puzzle : null;
            return new PuzzleSeedScanResult
            {
                Found = baked != null,
                Seed = seedValue,
                Status = result.Status,
                Puzzle = baked,
                Solution = baked != null ? new[] { result.Solution } : null,
                Routes = 0
            };
        }

        private static void Keep(ref PuzzleSeedScanResult best, PuzzleSeedScanResult candidate)
        {
            if (!IsBetter(candidate, best))
            {
                Object.DestroyImmediate(candidate.Puzzle);
                return;
            }

            if (best.Found)
                Object.DestroyImmediate(best.Puzzle);

            best = candidate;
        }

        private static PuzzlePlanetListAsset Planets(
            PuzzleSeed seed, SpaceShapingSettings settings, int seedValue)
        {
            PuzzleSeed probe = Object.Instantiate(seed);
            probe.seed = seedValue;
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(probe, settings);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(probe);
            return planets;
        }
    }
}
