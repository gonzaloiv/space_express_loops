using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleSeedBatchTests
    {
        [Test]
        public void Run_MatchesTwoSequentialScans()
        {
            List<FakeSeedSearch> created = new();
            PuzzleSeedBatch batch = new(Factory(created, passSeed: 6), 5, 2, 2);
            List<PuzzleSeedScanResult> parallel = batch.Run();
            List<PuzzleSeedScanResult> sequential = Sequential(5, 2, passSeed: 6);

            Assert.AreEqual(sequential.Count, parallel.Count);
            for (int i = 0; i < sequential.Count; i++)
                AssertSame(sequential[i], parallel[i]);
            AssertAllDisposed(created);
        }

        [Test]
        public void Run_StopOnFirst_KeepsSeedOrderAndStopsOnThePass()
        {
            List<FakeSeedSearch> created = new();
            PuzzleSeedBatch batch = new(
                Factory(created, passSeed: 6, delay: Delay(5, 80)), 5, 3, 3, stopOnFirst: true);

            List<PuzzleSeedScanResult> results = batch.Run();

            Assert.AreEqual(2, results.Count, "the batch should report the failing seed then the pass");
            Assert.AreEqual(5, results[0].Seed);
            Assert.IsFalse(results[0].Found);
            Assert.AreEqual(6, results[1].Seed);
            Assert.IsTrue(results[1].Found);
            AssertAllDisposed(created);
        }

        [Test]
        public void Run_CancelFromProgress_StopsAndDisposes()
        {
            List<FakeSeedSearch> created = new();
            PuzzleSeedBatch batch = new(Factory(created, passSeed: 6), 5, 4, 2);

            List<PuzzleSeedScanResult> results = batch.Run((done, total) =>
            {
                if (done == 1)
                    batch.Cancel();
            });

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(5, results[0].Seed);
            AssertAllDisposed(created);
        }

        private static Func<int, IPuzzleSeedSearch> Factory(
            List<FakeSeedSearch> created, int passSeed, Func<int, int> delay = null)
        {
            return seedValue =>
            {
                FakeSeedSearch search = new(seedValue, passSeed, delay != null ? delay(seedValue) : 0);
                created.Add(search);
                return search;
            };
        }

        private static Func<int, int> Delay(int seed, int milliseconds) =>
            value => value == seed ? milliseconds : 0;

        private static List<PuzzleSeedScanResult> Sequential(int start, int count, int passSeed)
        {
            List<PuzzleSeedScanResult> results = new();
            for (int i = 0; i < count; i++)
            {
                using FakeSeedSearch search = new(start + i, passSeed, 0);
                results.Add(search.Confirm(search.Run(default)));
            }

            return results;
        }

        private static void AssertSame(PuzzleSeedScanResult expected, PuzzleSeedScanResult actual)
        {
            Assert.AreEqual(expected.Seed, actual.Seed);
            Assert.AreEqual(expected.Found, actual.Found);
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.Kind, actual.Kind);
        }

        private static void AssertAllDisposed(List<FakeSeedSearch> created)
        {
            for (int i = 0; i < created.Count; i++)
                Assert.IsTrue(created[i].Disposed, $"seed {created[i].Seed} was not disposed");
        }

        private sealed class FakeSeedSearch : IPuzzleSeedSearch
        {
            private readonly int passSeed;
            private readonly int delayMilliseconds;

            public FakeSeedSearch(int seed, int passSeed, int delayMilliseconds)
            {
                Seed = seed;
                this.passSeed = passSeed;
                this.delayMilliseconds = delayMilliseconds;
            }

            public int Seed { get; }

            public bool Disposed { get; private set; }

            public FleetZoneResult Run(CancellationToken token, Action onZone = null)
            {
                if (delayMilliseconds > 0)
                    Thread.Sleep(delayMilliseconds);

                return default;
            }

            public PuzzleSeedScanResult Confirm(FleetZoneResult found) => new()
            {
                Seed = Seed,
                Found = Seed == passSeed,
                Status = Seed == passSeed ? PuzzleBakeStatus.Baked : PuzzleBakeStatus.NoSolution,
                Kind = PuzzleSolutionKind.Curve
            };

            public void Dispose() => Disposed = true;
        }
    }
}
