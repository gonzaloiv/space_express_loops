using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Runs several prepared fleet searches at once and confirms them on the main thread in seed order,
    // so a batch scan uses every core yet reports the same order as the sequential scan. The prepare
    // delegate is called on the main thread; only IPuzzleSeedSearch.Run travels to the workers.
    public sealed class PuzzleSeedBatch
    {
        private const int CancelWaitMilliseconds = 500;

        private readonly Func<int, IPuzzleSeedSearch> prepare;
        private readonly int startSeed;
        private readonly int count;
        private readonly int workers;
        private readonly bool stopOnFirst;

        private CancellationTokenSource cancel;

        public PuzzleSeedBatch(
            Func<int, IPuzzleSeedSearch> prepare, int startSeed, int count, int workers,
            bool stopOnFirst = false)
        {
            this.prepare = prepare ?? throw new ArgumentNullException(nameof(prepare));
            this.startSeed = startSeed;
            this.count = Math.Max(0, count);
            this.workers = Math.Max(1, workers);
            this.stopOnFirst = stopOnFirst;
        }

        public static int DefaultWorkers => Math.Max(1, Environment.ProcessorCount - 1);

        // Cancels the run from a progress callback; a no-op once Run has returned.
        public void Cancel() => cancel?.Cancel();

        // Results are in seed order, up to and including the first pass when stopOnFirst is set.
        public List<PuzzleSeedScanResult> Run(Action<int, int> onProgress = null)
        {
            List<Slot> pending = new();
            List<PuzzleSeedScanResult> results = new();
            using CancellationTokenSource source = new();
            cancel = source;
            try
            {
                Scan(results, pending, onProgress, source.Token);
            }
            finally
            {
                source.Cancel();
                cancel = null;
                Release(pending);
            }

            return results;
        }

        private void Scan(
            List<PuzzleSeedScanResult> results, List<Slot> pending,
            Action<int, int> onProgress, CancellationToken token)
        {
            int end = startSeed + count;
            int next = startSeed;
            for (int i = 0; i < count && !token.IsCancellationRequested; i++)
            {
                Fill(pending, ref next, end, token);
                if (pending.Count == 0)
                    return;

                Slot slot = pending[0];
                pending.RemoveAt(0);
                PuzzleSeedScanResult result = Confirm(slot);
                results.Add(result);
                onProgress?.Invoke(results.Count, count);
                if (stopOnFirst && result.Found)
                    return;
            }
        }

        private void Fill(List<Slot> pending, ref int next, int end, CancellationToken token)
        {
            while (pending.Count < workers && next < end && !token.IsCancellationRequested)
            {
                int seedValue = next++;
                IPuzzleSeedSearch search = prepare(seedValue);
                Task<FleetZoneResult> work = Task.Factory.StartNew(
                    () => search.Run(token), CancellationToken.None,
                    TaskCreationOptions.LongRunning, TaskScheduler.Default);
                pending.Add(new Slot(search, work));
            }
        }

        private static PuzzleSeedScanResult Confirm(Slot slot)
        {
            try
            {
                return slot.Search.Confirm(slot.Work.GetAwaiter().GetResult());
            }
            finally
            {
                slot.Search.Dispose();
            }
        }

        // A stop or a failure must not leave a search or its worker behind; the probe checks the token
        // between zones, so a bounded wait is enough to let it leave the current one.
        private static void Release(List<Slot> pending)
        {
            for (int i = 0; i < pending.Count; i++)
            {
                Slot slot = pending[i];
                try
                {
                    slot.Work.Wait(CancelWaitMilliseconds);
                }
                catch (AggregateException)
                {
                }

                slot.Search.Dispose();
            }

            pending.Clear();
        }

        private readonly struct Slot
        {
            public readonly IPuzzleSeedSearch Search;
            public readonly Task<FleetZoneResult> Work;

            public Slot(IPuzzleSeedSearch search, Task<FleetZoneResult> work)
            {
                Search = search;
                Work = work;
            }
        }
    }
}
