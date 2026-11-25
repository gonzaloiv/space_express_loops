using System;
using System.Threading;
using System.Threading.Tasks;
using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public enum FleetSearchOutcome
    {
        Completed,
        Cancelled,
        Failed
    }

    // One fleet seed sweep: which seed asset to vary, and for how many seeds.
    public readonly struct FleetSearchRequest
    {
        public readonly PuzzleSeed Seed;
        public readonly SpaceShapingSettings Settings;
        public readonly PuzzleBrief Brief;
        public readonly int StartSeed;
        public readonly int Count;

        public FleetSearchRequest(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleBrief brief, int startSeed, int count)
        {
            Seed = seed;
            Settings = settings;
            Brief = brief;
            StartSeed = startSeed;
            Count = Math.Max(1, count);
        }
    }

    // Runs the multi-ship probe on a Task so a fleet seed hunt never blocks the editor: the field and
    // the variant stay on the main thread, only the snapshot probe runs on the worker. One search at a
    // time process-wide. A recompile cancels the search, waits briefly for the worker and releases the
    // caller's assets, so the gate is never left locked.
    public sealed class FleetSearchRun
    {
        private const int WorkerWaitMilliseconds = 500;

        private static FleetSearchRun running;

        private readonly FleetSearchRequest request;
        private readonly PuzzleSimSettings sim;
        private readonly Func<int, PuzzleSeedScanResult, bool> onSeed;
        private readonly Action<FleetSearchOutcome> onDone;
        private readonly Action onRelease;
        private readonly CancellationTokenSource cancel = new();

        private PuzzleMultiShipSearch active;
        private Task<FleetZoneResult> task;
        private int index;
        private int seedsDone;
        private int zonesDone;
        private bool finished;
        private bool released;

        private FleetSearchRun(
            FleetSearchRequest request, Func<int, PuzzleSeedScanResult, bool> onSeed,
            Action<FleetSearchOutcome> onDone, Action onRelease)
        {
            this.request = request;
            this.onSeed = onSeed;
            this.onDone = onDone;
            this.onRelease = onRelease;
            sim = PuzzleSimSettings.From(request.Settings);
        }

        public static bool IsRunning => running != null;

        public int SeedsDone => seedsDone;

        public int SeedsTotal => request.Count;

        public int ZonesDone => Volatile.Read(ref zonesDone);

        public static FleetSearchRun Start(
            FleetSearchRequest request, Func<int, PuzzleSeedScanResult, bool> onSeed,
            Action<FleetSearchOutcome> onDone, Action onRelease = null)
        {
            if (running != null)
                throw new InvalidOperationException("A fleet search is already running.");

            FleetSearchRun run = new(request, onSeed, onDone, onRelease);
            running = run;
            AssemblyReloadEvents.beforeAssemblyReload += run.OnBeforeReload;
            EditorApplication.update += run.Poll;
            run.StartSeed();
            return run;
        }

        public void Cancel()
        {
            if (!finished)
                cancel.Cancel();
        }

        private void StartSeed()
        {
            try
            {
                StartSeedCore();
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                Finish(FleetSearchOutcome.Failed);
            }
        }

        private void StartSeedCore()
        {
            if (cancel.IsCancellationRequested)
            {
                Finish(FleetSearchOutcome.Cancelled);
                return;
            }

            int seedValue = request.StartSeed + index;
            PuzzlePlanetListAsset planets =
                PuzzleArtifactFactory.PlanetsFor(request.Seed, request.Settings, seedValue);
            active = PuzzleMultiShipSearch.Prepare(planets, sim, request.Brief, seedValue);
            UnityEngine.Object.DestroyImmediate(planets);

            zonesDone = 0;
            PuzzleMultiShipSearch search = active;
            CancellationToken token = cancel.Token;
            task = Task.Run(() => search.Run(token, CountZone));
        }

        private void CountZone() => Interlocked.Increment(ref zonesDone);

        private void Poll()
        {
            if (finished || task == null || !task.IsCompleted)
                return;

            Task<FleetZoneResult> done = task;
            task = null;
            if (done.IsFaulted)
            {
                Debug.LogException(done.Exception != null
                    ? done.Exception.GetBaseException()
                    : new InvalidOperationException("The fleet search task failed."));
                Finish(FleetSearchOutcome.Failed);
                return;
            }

            if (cancel.IsCancellationRequested)
            {
                Finish(FleetSearchOutcome.Cancelled);
                return;
            }

            int seedValue = request.StartSeed + index;
            PuzzleSeedScanResult result;
            try
            {
                result = active.ToScanResult(active.Confirm(done.Result));
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                Finish(FleetSearchOutcome.Failed);
                return;
            }

            DisposeActive();
            seedsDone++;
            index++;

            if (!SafeOnSeed(seedValue, result) || index >= request.Count)
            {
                Finish(FleetSearchOutcome.Completed);
                return;
            }

            StartSeed();
        }

        private bool SafeOnSeed(int seedValue, PuzzleSeedScanResult result)
        {
            try
            {
                return onSeed(seedValue, result);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                return false;
            }
        }

        private void Finish(FleetSearchOutcome outcome)
        {
            if (finished)
                return;

            finished = true;
            DisposeActive();
            EditorApplication.update -= Poll;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeReload;
            if (running == this)
                running = null;

            Release();
            try
            {
                onDone?.Invoke(outcome);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
        }

        // A recompile unloads the assemblies under the worker: stop it, give it a bounded moment to
        // leave the zone it is in, then release the assets. Do not call onDone; the windows are
        // rebuilt and must not touch dead state.
        private void OnBeforeReload()
        {
            finished = true;
            EditorApplication.update -= Poll;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeReload;
            cancel.Cancel();
            if (running == this)
                running = null;

            WaitForWorker();
            DisposeActive();
            Release();
        }

        private void WaitForWorker()
        {
            Task<FleetZoneResult> pending = task;
            if (pending == null)
                return;

            try
            {
                pending.Wait(WorkerWaitMilliseconds);
            }
            catch (AggregateException)
            {
            }
        }

        private void Release()
        {
            if (released)
                return;

            released = true;
            try
            {
                onRelease?.Invoke();
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
        }

        private void DisposeActive()
        {
            if (active == null)
                return;

            active.Dispose();
            active = null;
        }
    }
}
