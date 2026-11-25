using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSeedScanner
    {
        public static PuzzleSeedScanResult Find(
            PuzzleSeed seed, SpaceShapingSettings settings, int attempts,
            System.Action<int, int> onProgress = null)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            if (brief.IsMulti && brief.Mechanism != PuzzleMechanism.Any)
            {
                return new PuzzleSeedScanResult
                {
                    Seed = seed.seed,
                    Status = PuzzleBakeStatus.UnsupportedMechanism
                };
            }

            return Scan(seed, settings, sim, brief, attempts, onProgress);
        }

        private static PuzzleSeedScanResult Scan(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleSimSettings sim, PuzzleBrief brief,
            int attempts, System.Action<int, int> onProgress)
        {
            PuzzleSeedTally tally = new();
            for (int i = 0; i < attempts; i++)
            {
                PuzzleSeedScanResult candidate = Probe(seed, settings, sim, brief, seed.seed + i);
                tally.Add(candidate);
                onProgress?.Invoke(tally.Probes, attempts);
                if (!candidate.Found)
                    continue;

                Debug.Log($"[Puzzle seed] {candidate.Seed}: baked {candidate.Routes} route(s).");
                break;
            }

            return tally.Result();
        }

        private static PuzzleSeedScanResult Probe(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleSimSettings sim, PuzzleBrief brief,
            int seedValue)
        {
            PuzzlePlanetListAsset planets = Planets(seed, settings, seedValue);
            if (brief.IsMulti)
                return ProbeMulti(planets, sim, brief, seedValue);

            PuzzleBakeResult baked = PuzzleFieldBake.Bake(planets, sim, brief);
            Object.DestroyImmediate(planets);
            return new PuzzleSeedScanResult
            {
                Found = baked.Baked,
                Seed = seedValue,
                Status = baked.Status,
                Puzzle = baked.Puzzle,
                Solution = baked.Solution,
                Kind = baked.Kind,
                RunReport = baked.RunReport,
                Routes = baked.Baked ? baked.Puzzle.routes : 0
            };
        }

        // The field is copied into the search on the main thread, so the planets asset can go at once.
        private static PuzzleSeedScanResult ProbeMulti(
            PuzzlePlanetListAsset planets, PuzzleSimSettings sim, PuzzleBrief brief, int seedValue)
        {
            PuzzleMultiShipSearch search = PuzzleMultiShipSearch.Prepare(planets, sim, brief, seedValue);
            Object.DestroyImmediate(planets);
            PuzzleSeedScanResult result = search.ToScanResult(search.Confirm(search.Run()));
            search.Dispose();
            return result;
        }

        private static PuzzlePlanetListAsset Planets(
            PuzzleSeed seed, SpaceShapingSettings settings, int seedValue) =>
            PuzzleArtifactFactory.PlanetsFor(seed, settings, seedValue);
    }
}
