using System.Threading;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // One multi-ship bake split so the probe can run off the editor thread: Prepare and Confirm run on
    // the main thread (the field is a ScriptableObject), Run is the only part the worker calls.
    public sealed class PuzzleMultiShipSearch
    {
        private readonly PuzzleData field;
        private readonly FleetField fleet;
        private readonly PuzzleBrief brief;

        private PuzzleMultiShipSearch(
            PuzzleData field, FleetField fleet, PuzzleBrief brief, int seed, PuzzleBakeStatus status)
        {
            this.field = field;
            this.fleet = fleet;
            this.brief = brief;
            Seed = seed;
            Status = status;
        }

        public int Seed { get; }

        public PuzzleBakeStatus Status { get; }

        public bool Prepared => Status == PuzzleBakeStatus.Baked;

        public static PuzzleMultiShipSearch Prepare(
            PuzzlePlanetListAsset planets, PuzzleSimSettings settings, PuzzleBrief brief, int seed)
        {
            if (!brief.IsMulti)
                return new PuzzleMultiShipSearch(null, null, brief, seed, PuzzleBakeStatus.UnsupportedBrief);

            if (brief.Mechanism != PuzzleMechanism.Any)
                return new PuzzleMultiShipSearch(
                    null, null, brief, seed, PuzzleBakeStatus.UnsupportedMechanism);

            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            if (!PuzzleFieldBake.Apply(brief, field, planets, settings))
            {
                Object.DestroyImmediate(field);
                return new PuzzleMultiShipSearch(null, null, brief, seed, PuzzleBakeStatus.NoRun);
            }

            return new PuzzleMultiShipSearch(
                field, FleetField.From(field, settings), brief, seed, PuzzleBakeStatus.Baked);
        }

        // The worker's whole job: plain data in, zone and rescued runs out.
        public FleetZoneResult Run(CancellationToken token = default, System.Action onZone = null) =>
            Prepared ? new PuzzleMultiShipProbe(fleet, brief).Find(token, onZone) : default;

        // Back on the main thread: select the ships, re-check the rescue, build the variant.
        public PuzzleMultiShipResult Confirm(FleetZoneResult found)
        {
            if (!Prepared)
                return new PuzzleMultiShipResult { Status = Status };

            if (!found.Found)
                return new PuzzleMultiShipResult { Status = PuzzleBakeStatus.NoSolution };

            return PuzzleMultiShipBake.ConfirmZone(field, fleet, found, brief);
        }

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
                Kind = baked.Kind
            };
        }

        public void Dispose()
        {
            if (field != null)
                Object.DestroyImmediate(field);
        }
    }
}
