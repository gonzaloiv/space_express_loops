using System;
using System.Threading;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // One seed's prepared fleet search: Run is the worker's whole job, Confirm is back on the main
    // thread. The batch uses this seam so its scheduling can be tested without baking a field.
    public interface IPuzzleSeedSearch : IDisposable
    {
        FleetZoneResult Run(CancellationToken token, Action onZone = null);

        PuzzleSeedScanResult Confirm(FleetZoneResult found);
    }
}
