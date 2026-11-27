using System.Threading;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // One multi-ship bake split so the probe can run off the editor thread: Prepare and Confirm run on
    // the main thread (the field is a ScriptableObject), Run is the only part the worker calls.
    public sealed class PuzzleMultiShipSearch : IPuzzleSeedSearch
    {
        private readonly PuzzleData field;
        private readonly FleetField fleet;
        private readonly PuzzleBrief brief;
        private readonly PuzzleScanTimings timings;

        private PuzzleMultiShipSearch(
            PuzzleData field, FleetField fleet, PuzzleBrief brief, int seed, PuzzleBakeStatus status,
            PuzzleScanTimings timings)
        {
            this.field = field;
            this.fleet = fleet;
            this.brief = brief;
            this.timings = timings;
            Seed = seed;
            Status = status;
        }

        public int Seed { get; }

        public PuzzleBakeStatus Status { get; }

        public bool Prepared => Status == PuzzleBakeStatus.Baked;

        public static PuzzleMultiShipSearch Prepare(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief, int seed,
            PuzzleScanTimings timings = null)
        {
            if (!brief.IsMulti)
                return new PuzzleMultiShipSearch(
                    null, null, brief, seed, PuzzleBakeStatus.UnsupportedBrief, timings);

            if (!brief.SupportsFleetRole)
                return new PuzzleMultiShipSearch(
                    null, null, brief, seed, PuzzleBakeStatus.UnsupportedMechanism, timings);

            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            if (!PuzzleFieldBake.Apply(brief, field, planets, settings, out _, timings))
            {
                Object.DestroyImmediate(field);
                return new PuzzleMultiShipSearch(
                    null, null, brief, seed, PuzzleBakeStatus.NoRun, timings);
            }

            return new PuzzleMultiShipSearch(
                field, FleetField.From(field, settings), brief, seed, PuzzleBakeStatus.Baked, timings);
        }

        // A batch's preparation: the planets are made and released on the main thread, and the field
        // stays for the confirm after the worker returns.
        public static PuzzleMultiShipSearch PrepareForSeed(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleBrief brief, int seedValue,
            PuzzleScanTimings timings = null)
        {
            long started = timings != null ? timings.Start() : 0;
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.PlanetsFor(seed, settings, seedValue);
            timings?.Add(PuzzleScanTimings.Setup, started);
            PuzzleMultiShipSearch search =
                Prepare(planets, PuzzleSimSettings.From(settings), brief, seedValue, timings);
            Object.DestroyImmediate(planets);
            return search;
        }

        // The worker's whole job: plain data in, zone and rescued runs out.
        public FleetZoneResult Run(CancellationToken token = default, System.Action onZone = null) =>
            Prepared ? new PuzzleMultiShipProbe(fleet, brief, timings).Find(token, onZone) : default;

        // Back on the main thread: select the ships, re-check the rescue, build the variant.
        public PuzzleMultiShipResult Confirm(FleetZoneResult found)
        {
            if (!Prepared)
                return new PuzzleMultiShipResult { Status = Status };

            if (!found.Found)
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.NoSolution };

            return PuzzleMultiShipBake.ConfirmZone(field, fleet, found, brief, timings);
        }

        // The batch's view: confirm and map to the scan result in one step.
        PuzzleSeedScanResult IPuzzleSeedSearch.Confirm(FleetZoneResult found) =>
            ToScanResult(Confirm(found));

        public PuzzleSeedScanResult ToScanResult(PuzzleMultiShipResult baked)
        {
            PuzzleData variant = baked.Status == PuzzleBakeStatus.Baked ? baked.Puzzle : null;
            return new PuzzleSeedScanResult
            {
                Found = variant != null,
                Seed = Seed,
                Status = baked.Status,
                Puzzle = variant,
                Solution = variant != null ? new[] { baked.Solution } : null,
                Kind = baked.Kind,
                Timings = timings
            };
        }

        public void Dispose()
        {
            if (field != null)
                Object.DestroyImmediate(field);
        }
    }
}
