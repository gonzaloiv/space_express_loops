using System;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The seed scan's running summary, shared by the synchronous search and the background one: how
    // many seeds were probed, which baked, and how the failures split by status.
    public sealed class PuzzleSeedTally
    {
        private static readonly int StatusCount =
            Enum.GetValues(typeof(PuzzleBakeStatus)).Length;

        private readonly int[] failures = new int[StatusCount];
        private PuzzleSeedScanResult report;
        private bool hasReport;

        public PuzzleSeedScanResult Found { get; private set; }

        public int Probes { get; private set; }

        public int Baked { get; private set; }

        public bool HasFailure { get; private set; }

        public PuzzleBakeStatus FirstFailure { get; private set; }

        public void Add(PuzzleSeedScanResult candidate)
        {
            Probes++;
            if (candidate.Found)
            {
                Baked++;
                Found = candidate;
                return;
            }

            failures[(int)candidate.Status]++;
            if (!hasReport)
            {
                hasReport = true;
                report = candidate;
            }

            if (HasFailure)
                return;

            HasFailure = true;
            FirstFailure = candidate.Status;
        }

        public PuzzleSeedScanResult Result()
        {
            PuzzleSeedScanResult result = Found.Found ? Found : report;
            result.Attempts = Probes;
            result.Baked = Baked;
            result.Failures = failures;
            result.HasFailure = HasFailure;
            result.FirstFailure = FirstFailure;
            return result;
        }
    }
}
