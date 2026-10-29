using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleClockSearch
    {
        private const float ShiftFactor = 1.5f;

        public static bool TrySelect(
            PuzzleData puzzle, PuzzleSimSettings settings, PuzzleBrief brief, out PuzzleSolution solution)
        {
            solution = default;
            if (!PuzzleWindow.TryCrossing(puzzle, settings, out float crossing))
                return false;

            float halfWidth = PuzzleWindow.HalfWidth(crossing, brief.WindowTightness);
            if (halfWidth <= 0f)
                return false;

            if (!TryBareTime(puzzle, settings, out float bareTime))
                return false;

            ClockScan scan = new(puzzle, settings, bareTime, ShiftFactor * halfWidth);
            Vector3[] centres = PuzzleZoneCandidates.ClockCentres(puzzle);
            bool found = false;
            for (int c = 0; c < centres.Length; c++)
                ScanCentre(scan, centres[c], ref solution, ref found);

            return found;
        }

        private static void ScanCentre(
            ClockScan scan, Vector3 centre, ref PuzzleSolution solution, ref bool found)
        {
            float[] sizes = PuzzleZoneCandidates.Sizes(scan.Puzzle);
            float[] scales = PuzzleZoneCandidates.Scales(scan.Settings, PuzzleZoneCandidates.DefaultSamples);
            for (int s = 0; s < sizes.Length; s++)
            {
                for (int i = 0; i < scales.Length; i++)
                {
                    PuzzleZone zone = PuzzleZoneCandidates.Zone(
                        centre, sizes[s], scales[i], scan.Settings.ZoneFalloff);
                    if (PuzzleZoneCandidates.ContainsEndpoint(zone, scan.Puzzle))
                        continue;

                    if (!IsClockCandidate(scan, zone, centre, out PuzzleSolution candidate))
                        continue;

                    if (!found || PuzzleSolutionOrdering.Precedes(candidate, solution, scan.BareTime))
                    {
                        solution = candidate;
                        found = true;
                    }
                }
            }
        }

        private static bool IsClockCandidate(
            ClockScan scan, PuzzleZone zone, Vector3 centre, out PuzzleSolution solution)
        {
            if (!PuzzleZoneCandidates.ContainsOnlySource(zone, scan.Puzzle, centre))
            {
                solution = default;
                return false;
            }

            if (!PuzzleZoneRun.TryArrive(scan.Puzzle, scan.Settings, zone, out solution))
                return false;

            return Mathf.Abs(solution.ShipTime - scan.BareTime) >= scan.RequiredShift;
        }

        private static bool TryBareTime(PuzzleData puzzle, PuzzleSimSettings settings, out float shipTime)
        {
            SpaceShapingSession session = new(SpaceShapingSetup.Build(puzzle, settings));
            TrajectoryResult result = session.Simulate();
            shipTime = result.ShipTime;
            return result.Outcome == TrajectoryOutcome.Arrive;
        }

        private readonly struct ClockScan
        {
            public readonly PuzzleData Puzzle;
            public readonly PuzzleSimSettings Settings;
            public readonly float BareTime;
            public readonly float RequiredShift;

            public ClockScan(PuzzleData puzzle, PuzzleSimSettings settings, float bareTime, float requiredShift)
            {
                Puzzle = puzzle;
                Settings = settings;
                BareTime = bareTime;
                RequiredShift = requiredShift;
            }
        }
    }
}
