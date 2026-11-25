using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A movable job's shipped frame must be the one Place would pick, keep clear of every ship's
    // endpoints, and still need moving: scaling it in place must not solve the job.
    public static class PuzzleStartFrameCheck
    {
        public static void CheckMovable(
            PuzzleData puzzle, PuzzleJobContract contract, PuzzleSimSettings? settings,
            List<string> failures)
        {
            if (!HasFrame(puzzle, failures))
                return;

            PuzzleFrame frame = puzzle.frames[0];
            if (!AtPlacedSpot(puzzle, contract, settings, frame, failures))
                return;

            if (CoversEndpoint(puzzle))
            {
                failures.Add("start frame covers an endpoint");
                return;
            }

            if (settings.HasValue
                && PuzzleStartFrame.SolvesInPlace(puzzle, settings.Value, frame.centre, frame.size))
                failures.Add("the start solves in place");
        }

        private static bool HasFrame(PuzzleData puzzle, List<string> failures)
        {
            if (puzzle.frames != null && puzzle.frames.Length > 0)
                return true;

            failures.Add("no start frame");
            return false;
        }

        private static bool AtPlacedSpot(
            PuzzleData puzzle, PuzzleJobContract contract, PuzzleSimSettings? settings,
            PuzzleFrame frame, List<string> failures)
        {
            Vector3 spot = PuzzleFieldBake.StartSpot(puzzle, contract.StartRule);
            Vector3 expected = settings.HasValue
                ? PuzzleStartFrame.Place(puzzle, settings.Value, spot, frame.size)
                : spot;
            if ((frame.centre - expected).sqrMagnitude <= 0.000001f)
                return true;

            failures.Add($"start frame {frame.centre}, expected {expected}");
            return false;
        }

        public static bool CoversEndpoint(PuzzleData puzzle)
        {
            if (puzzle.frames == null || puzzle.frames.Length == 0)
                return false;

            PuzzleZone frame = new()
            {
                centre = puzzle.frames[0].centre,
                halfExtents = puzzle.frames[0].BoxSize * 0.5f,
                scale = 1f
            };
            PuzzleEndpoints endpoints = puzzle.Endpoints;
            for (int i = 0; i < endpoints.Count; i++)
            {
                if (frame.Contains(endpoints[i]))
                    return true;
            }

            return false;
        }
    }
}
