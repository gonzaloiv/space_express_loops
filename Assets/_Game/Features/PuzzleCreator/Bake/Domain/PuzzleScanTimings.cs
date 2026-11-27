using System.Diagnostics;
using System.Threading;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // What one seed's scan spends its time on. Only the editor's seed scan creates one; the pinned
    // bake and the Creator tests pass null, so their bake pays a null check and nothing else. The
    // probe records from a worker, so the phase totals and the simulation count are interlocked.
    public sealed class PuzzleScanTimings
    {
        // Planet generation, field apply and the run picker: the work before the search starts.
        public const int Setup = 0;

        // The pristine field and the cached start arrivals.
        public const int StartCache = 1;

        // The zone sweep's RescuedFixed calls.
        public const int Probe = 2;

        // The solution search and the shorter-zone check (singles), or the ship selection (fleets).
        public const int Select = 3;

        // The variant, verifier and the role/sun/route checks.
        public const int Confirm = 4;

        // PuzzleSolveWindow.Measure, on a passing seed only.
        public const int Window = 5;

        public const int PhaseCount = 6;

        private static readonly string[] Names = { "setup", "start", "probe", "select", "confirm", "window" };

        private readonly long[] phaseTicks = new long[PhaseCount];
        private int simulations;

        public static string Name(int phase) => Names[phase];

        public int Simulations => Volatile.Read(ref simulations);

        public long Ticks(int phase) => Interlocked.Read(ref phaseTicks[phase]);

        public double Milliseconds(int phase) => Ticks(phase) * 1000.0 / Stopwatch.Frequency;

        public long Start() => Stopwatch.GetTimestamp();

        public void Add(int phase, long started) =>
            Interlocked.Add(ref phaseTicks[phase], Stopwatch.GetTimestamp() - started);

        public void CountSimulation() => Interlocked.Increment(ref simulations);
    }
}
