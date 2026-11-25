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
    // time process-wide, and a recompile cancels the search before the domain goes away.
    public sealed class FleetSearchRun
    {
        private static FleetSearchRun running;

        private readonly FleetSearchRequest request;
        private readonly PuzzleSimSettings sim;
        private readonly Func<int, PuzzleSeedScanResult, bool> onSeed;
        private readonly Action<FleetSearchOutcome> onDone;
        private readonly CancellationTokenSource cancel = new();

        private PuzzleMultiShipSearch active;
        private Task<FleetZoneResult> task;
        private int index;
        private int seedsDone;
        private int zonesDone;
        private bool finished;

        private FleetSearchRun(
            FleetSearchRequest request, Func<int, PuzzleSeedScanResult, bool> onSeed,
            Action<FleetSearchOutcome> onDone)
        {
            this.request = request;
            this.onSeed = onSeed;
            this.onDone = onDone;
            sim = PuzzleSimSettings.From(request.Settings);
        }

        public static bool IsRunning => running != null;

        public int SeedsDone => seedsDone;

        public int SeedsTotal => request.Count;

        public int ZonesDone => Volatile.Read(ref zonesDone);

        public static FleetSearchRun Start(
            FleetSearchRequest request, Func<int, PuzzleSeedScanResult, bool> onSeed,
            Action<FleetSearchOutcome> onDone)
        {
            if (running != null)
                throw new InvalidOperationException("A fleet search is already running.");

            FleetSearchRun run = new(request, onSeed, onDone);
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
            if (done.IsFaulted || cancel.IsCancellationRequested)
            {
                Finish(done.IsFaulted ? FleetSearchOutcome.Failed : FleetSearchOutcome.Cancelled);
                return;
            }

            int seedValue = request.StartSeed + index;
            PuzzleSeedScanResult result = active.ToScanResult(active.Confirm(done.Result));
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

            try
            {
                onDone?.Invoke(outcome);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
            }
        }

        private void OnBeforeReload()
        {
            finished = true;
            EditorApplication.update -= Poll;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeReload;
            cancel.Cancel();
            if (running == this)
                running = null;
            DisposeActive();
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
