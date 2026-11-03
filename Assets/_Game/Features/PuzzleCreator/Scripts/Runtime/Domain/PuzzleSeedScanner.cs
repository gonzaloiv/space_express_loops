using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSeedScanner
    {
        private static readonly int StatusCount =
            System.Enum.GetValues(typeof(PuzzleBakeStatus)).Length;

        public static bool IsBetter(PuzzleSeedScanResult candidate, PuzzleSeedScanResult best) =>
            candidate.Found && (!best.Found || candidate.Routes > best.Routes);

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
            PuzzleSeedScanResult best = new() { Seed = seed.seed };
            Tally tally = new() { Failures = new int[StatusCount] };
            for (int i = 0; i < attempts; i++)
            {
                PuzzleSeedScanResult candidate = Probe(seed, settings, sim, brief, seed.seed + i);
                tally.Probes++;
                onProgress?.Invoke(tally.Probes, attempts);
                if (!candidate.Found)
                {
                    Record(candidate, ref best, ref tally);
                    continue;
                }

                tally.Baked++;
                best = candidate;
                Debug.Log($"[Puzzle seed] {candidate.Seed}: baked {candidate.Routes} route(s).");
                break;
            }

            return WithReport(best, tally);
        }

        private static void Record(
            PuzzleSeedScanResult candidate, ref PuzzleSeedScanResult best, ref Tally tally)
        {
            tally.Failures[(int)candidate.Status]++;
            if (!best.Found)
            {
                best.Status = candidate.Status;
                best.Kind = candidate.Kind;
            }
            if (tally.HasFailure)
                return;

            tally.HasFailure = true;
            tally.FirstFailure = candidate.Status;
        }

        private static PuzzleSeedScanResult WithReport(PuzzleSeedScanResult best, Tally tally)
        {
            best.Attempts = tally.Probes;
            best.Baked = tally.Baked;
            best.Failures = tally.Failures;
            best.HasFailure = tally.HasFailure;
            best.FirstFailure = tally.FirstFailure;
            return best;
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
                Routes = baked.Baked ? baked.Puzzle.routes : 0
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
                Kind = result.Kind,
                Routes = 0
            };
        }

        private static PuzzlePlanetListAsset Planets(
            PuzzleSeed seed, SpaceShapingSettings settings, int seedValue) =>
            PuzzleArtifactFactory.PlanetsFor(seed, settings, seedValue);

        private struct Tally
        {
            public int Probes;
            public int Baked;
            public int[] Failures;
            public bool HasFailure;
            public PuzzleBakeStatus FirstFailure;
        }
    }
}
