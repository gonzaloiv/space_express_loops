using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The fleet's chord picker: one disjoint pair per ship, the chords' midpoint centre, and the
    // biggest decoy whose block band reaches every chord (the shared planet the lesson moves there).
    public static class PuzzleConstructFleetChord
    {
        private const int PairCandidates = 24;

        // Only Separate is built here; fan-out/converge have their own plans.
        public static bool TryPickShips(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request,
            out PuzzleConstructedShip[] ships, out int hubIndex, out Vector3 hubHome)
        {
            ships = null;
            hubIndex = -1;
            hubHome = Vector3.zero;
            int count = request.Brief.Ships;
            if (count < 2 || request.Brief.Relation != PuzzleShipRelation.Separate)
                return false;

            if (!Matching(planets, request, count, out Pair[] pairs))
                return false;

            ships = Ships(pairs, planets);
            hubHome = Centre(ships);
            hubIndex = Hub(planets, ships, hubHome);
            return hubIndex >= 0;
        }

        private static bool Matching(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int count,
            out Pair[] pairs)
        {
            pairs = null;
            List<Pair> candidates = Candidates(planets, request);
            if (candidates.Count < count)
                return false;

            candidates.Sort((a, b) => a.Error.CompareTo(b.Error));
            if (candidates.Count > PairCandidates)
                candidates.RemoveRange(PairCandidates, candidates.Count - PairCandidates);

            Pair[] picked = new Pair[count];
            pairs = Pick(candidates, 0, picked, 0, new bool[planets.Length]) ? picked : null;
            return pairs != null;
        }

        private static List<Pair> Candidates(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request)
        {
            List<Pair> candidates = new();
            float target = request.Field.RunDistance;
            for (int i = 0; i < planets.Length; i++)
            {
                for (int j = i + 1; j < planets.Length; j++)
                {
                    if (!PuzzleConstructChord.PairFits(planets, request, i, j))
                        continue;

                    float length = Vector3.Distance(planets[i].position, planets[j].position);
                    candidates.Add(new Pair { I = i, J = j, Error = Mathf.Abs(length - target) });
                }
            }

            return candidates;
        }

        private static bool Pick(
            List<Pair> candidates, int start, Pair[] picked, int taken, bool[] used)
        {
            if (taken == picked.Length)
                return true;

            int remaining = picked.Length - taken;
            for (int i = start; i <= candidates.Count - remaining; i++)
            {
                Pair pair = candidates[i];
                if (used[pair.I] || used[pair.J])
                    continue;

                used[pair.I] = true;
                used[pair.J] = true;
                picked[taken] = pair;
                if (Pick(candidates, i + 1, picked, taken + 1, used))
                    return true;
                used[pair.I] = false;
                used[pair.J] = false;
            }

            return false;
        }

        private static PuzzleConstructedShip[] Ships(Pair[] pairs, PuzzleFieldPlanet[] planets)
        {
            PuzzleConstructedShip[] ships = new PuzzleConstructedShip[pairs.Length];
            for (int i = 0; i < pairs.Length; i++)
            {
                int origin = Mathf.Min(pairs[i].I, pairs[i].J);
                int destination = Mathf.Max(pairs[i].I, pairs[i].J);
                ships[i] = new PuzzleConstructedShip
                {
                    OriginIndex = origin,
                    DestinationIndex = destination,
                    Origin = planets[origin].position,
                    Destination = planets[destination].position,
                    CaptureRadius = planets[destination].radius
                };
            }

            return ships;
        }

        private static Vector3 Centre(PuzzleConstructedShip[] ships)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < ships.Length; i++)
                sum += (ships[i].Origin + ships[i].Destination) * 0.5f;
            return sum / ships.Length;
        }

        private static int Hub(
            PuzzleFieldPlanet[] planets, PuzzleConstructedShip[] ships, Vector3 home)
        {
            float maxDistance = MaxDistance(ships, home);
            bool[] used = Used(planets, ships);
            int best = -1;
            float bestRadius = 0f;
            for (int i = 0; i < planets.Length; i++)
            {
                if (used[i]
                    || planets[i].radius * PuzzleRunChoice.PlanetBlockBandFactor < maxDistance)
                    continue;
                if (planets[i].radius <= bestRadius)
                    continue;

                bestRadius = planets[i].radius;
                best = i;
            }

            return best;
        }

        private static float MaxDistance(PuzzleConstructedShip[] ships, Vector3 home)
        {
            float max = 0f;
            for (int i = 0; i < ships.Length; i++)
            {
                max = Mathf.Max(max, PuzzleRunGeometry.DistanceToSegment(
                    home, ships[i].Origin, ships[i].Destination));
            }

            return max;
        }

        private static bool[] Used(PuzzleFieldPlanet[] planets, PuzzleConstructedShip[] ships)
        {
            bool[] used = new bool[planets.Length];
            for (int i = 0; i < ships.Length; i++)
            {
                used[ships[i].OriginIndex] = true;
                used[ships[i].DestinationIndex] = true;
            }

            return used;
        }

        private struct Pair
        {
            public int I;
            public int J;
            public float Error;
        }
    }
}
