using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSeedScanner
    {
        public static PuzzleSeedScanResult Find(
            PuzzleSeed seed, SpaceShapingSettings settings, int attempts,
            System.Action<int, int> onProgress = null, PuzzleScanTimings timings = null)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            if (brief.IsMulti && !brief.SupportsFleetRole)
            {
                return new PuzzleSeedScanResult
                {
                    Seed = seed.seed,
                    Status = PuzzleBakeStatus.UnsupportedMechanism,
                    Timings = timings
                };
            }

            return Scan(seed, settings, sim, brief, attempts, onProgress, timings);
        }

        private static PuzzleSeedScanResult Scan(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleSimSettings sim, PuzzleBrief brief,
            int attempts, System.Action<int, int> onProgress, PuzzleScanTimings timings)
        {
            PuzzleSeedTally tally = new();
            for (int i = 0; i < attempts; i++)
            {
                PuzzleSeedScanResult candidate =
                    Probe(seed, settings, sim, brief, seed.seed + i, timings);
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
            int seedValue, PuzzleScanTimings timings)
        {
            if (brief.IsMulti)
                return ProbeMulti(seed, settings, brief, seedValue, timings);

            long started = timings != null ? timings.Start() : 0;
            PuzzlePlanetListAsset planets = Planets(seed, settings, seedValue);
            timings?.Add(PuzzleScanTimings.Setup, started);
            PuzzleBakeResult baked = PuzzleFieldBake.Bake(planets, sim, brief, timings);
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
                Timings = timings,
                Routes = baked.Baked ? baked.Puzzle.routes : 0
            };
        }

        // The field is copied into the search on the main thread, so the planets asset can go at once.
        private static PuzzleSeedScanResult ProbeMulti(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleBrief brief, int seedValue,
            PuzzleScanTimings timings)
        {
            PuzzleMultiShipSearch search =
                PuzzleMultiShipSearch.PrepareForSeed(seed, settings, brief, seedValue, timings);
            PuzzleSeedScanResult result = search.ToScanResult(search.Confirm(search.Run()));
            search.Dispose();
            return result;
        }

        private static PuzzlePlanetListAsset Planets(
            PuzzleSeed seed, SpaceShapingSettings settings, int seedValue) =>
            PuzzleArtifactFactory.PlanetsFor(seed, settings, seedValue);
    }
}
