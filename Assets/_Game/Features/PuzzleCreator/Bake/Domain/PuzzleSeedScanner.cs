using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleSeedScanner
    {
        // The contract decides the generator; the mechanism picks the constructor's recipe, and an
        // explicit lesson overrides it (the Creator tests pass one directly).
        public static PuzzleSeedScanResult Find(
            PuzzleJobContract job, PuzzleSeed seed, SpaceShapingSettings settings, int attempts,
            IPuzzleLesson lesson = null, System.Action<int, int> onProgress = null)
        {
            if (job.Generator == PuzzleGenerator.Construct)
                return Construct(job, seed, settings, attempts, lesson, onProgress);

            return Find(seed, settings, attempts, onProgress);
        }

        public static PuzzleSeedScanResult Find(
            PuzzleSeed seed, SpaceShapingSettings settings, int attempts,
            System.Action<int, int> onProgress = null)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            if (brief.IsMulti && !brief.SupportsFleetRole)
            {
                return new PuzzleSeedScanResult
                {
                    Seed = seed.seed,
                    Status = PuzzleBakeStatus.UnsupportedMechanism
                };
            }

            return Scan(seed, settings, sim, brief, attempts, onProgress);
        }

        private static PuzzleSeedScanResult Construct(
            PuzzleJobContract job, PuzzleSeed seed, SpaceShapingSettings settings, int attempts,
            IPuzzleLesson lesson, System.Action<int, int> onProgress)
        {
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            int retries = Mathf.Max(1, attempts);
            PuzzleConstructRequest request = new()
            {
                Contract = job,
                Brief = brief,
                Seed = PuzzleSeedFields.From(seed, settings),
                Field = PuzzleFieldSettings.From(settings),
                Sim = PuzzleSimSettings.From(settings),
                Lesson = lesson ?? PuzzleLessonChoice.For(brief)
            };
            PuzzleBakeResult baked = PuzzleConstructBake.Bake(request, retries);
            onProgress?.Invoke(retries, retries);
            return Result(request.Seed.Seed, baked);
        }

        private static PuzzleSeedScanResult Result(int seed, PuzzleBakeResult baked) => new()
        {
            Found = baked.Baked,
            Seed = seed,
            Status = baked.Status,
            Puzzle = baked.Puzzle,
            Solution = baked.Solution,
            Kind = baked.Kind,
            RunReport = baked.RunReport,
            Routes = baked.Baked ? baked.Puzzle.routes : 0,
            Window = baked.Window,
            ContractChecked = baked.Baked || baked.Problems != null,
            Problems = baked.Problems
        };

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
            if (brief.IsMulti)
                return ProbeMulti(seed, settings, brief, seedValue);

            PuzzlePlanetListAsset planets = Planets(seed, settings, seedValue);
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
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleBrief brief, int seedValue)
        {
            PuzzleMultiShipSearch search =
                PuzzleMultiShipSearch.PrepareForSeed(seed, settings, brief, seedValue);
            PuzzleSeedScanResult result = search.ToScanResult(search.Confirm(search.Run()));
            search.Dispose();
            return result;
        }

        private static PuzzlePlanetListAsset Planets(
            PuzzleSeed seed, SpaceShapingSettings settings, int seedValue) =>
            PuzzleArtifactFactory.PlanetsFor(seed, settings, seedValue);
    }
}
