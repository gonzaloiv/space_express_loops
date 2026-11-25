using System.Collections.Generic;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // What the off-thread probe found: the zone, the runs it rescues, and the scan the main thread
    // re-uses to confirm them.
    public readonly struct FleetZoneResult
    {
        public readonly bool Found;
        public readonly PuzzleZone Zone;
        public readonly List<PuzzleShipRescue> Rescued;
        public readonly PuzzleShipScan Scan;

        public FleetZoneResult(
            bool found, PuzzleZone zone, List<PuzzleShipRescue> rescued, PuzzleShipScan scan)
        {
            Found = found;
            Zone = zone;
            Rescued = rescued;
            Scan = scan;
        }
    }
}
