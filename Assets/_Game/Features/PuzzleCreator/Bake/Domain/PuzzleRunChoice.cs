using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleRunChoice
    {
        private const float MinRunFraction = 0.5f;
        private const float MaxRunFactor = 1.5f;
        internal const float SunBlockBandFactor = 2f;
        internal const float PlanetBlockBandFactor = 2f;

        public static bool TryPick(
            PuzzleRunRequest request, out Vector3 origin, out Vector3 destination) =>
            TryPick(request, out origin, out destination, out _);

        public static bool TryPickCandidates(
            PuzzleRunRequest request, int count, out Candidate[] candidates)
        {
            List<Candidate> found = new();
            for (int i = 0; i < request.Planets.Length; i++)
            {
                for (int j = i + 1; j < request.Planets.Length; j++)
                    AddCandidate(i, j, request, found);
            }

            found.Sort((a, b) => a.Delta.CompareTo(b.Delta));
            int length = Mathf.Min(count, found.Count);
            candidates = new Candidate[length];
            for (int i = 0; i < length; i++)
                candidates[i] = found[i];

            return length > 0;
        }

        private static void AddCandidate(
            int i, int j, PuzzleRunRequest request, List<Candidate> found)
        {
            Vector3 from = request.Planets[i].position;
            Vector3 to = request.Planets[j].position;
            float run = Vector3.Distance(from, to);
            if (run < request.TargetRun * MinRunFraction || run > request.TargetRun * MaxRunFactor)
                return;
            if (!EndpointsClear(request, i, j))
                return;
            if (!request.AllowSunOnChord && SunBlocked(request, from, to))
                return;

            found.Add(new Candidate(from, to, Mathf.Abs(run - request.TargetRun)));
        }

        public static bool TryPick(
            PuzzleRunRequest request, out Vector3 origin, out Vector3 destination,
            out PuzzleRunReport report)
        {
            RunChoice choice = new() { Best = float.MaxValue };
            if (request.HasCandidate)
            {
                int ci = IndexOf(request.Planets, request.CandidateOrigin);
                int cj = IndexOf(request.Planets, request.CandidateDestination);
                if (ci >= 0 && cj >= 0 && ci != cj)
                    Consider(ci, cj, request, ref choice);

                origin = choice.Origin;
                destination = choice.Destination;
                report = choice.Report;
                return choice.Found;
            }

            for (int i = 0; i < request.Planets.Length; i++)
            {
                for (int j = i + 1; j < request.Planets.Length; j++)
                    Consider(i, j, request, ref choice);
            }

            origin = choice.Origin;
            destination = choice.Destination;
            report = choice.Report;
            return choice.Found;
        }

        private static void Consider(
            int i, int j, PuzzleRunRequest request, ref RunChoice choice)
        {
            choice.Report.Pairs++;
            Vector3 from = request.Planets[i].position;
            Vector3 to = request.Planets[j].position;
            float run = Vector3.Distance(from, to);
            if (run < request.TargetRun * MinRunFraction || run > request.TargetRun * MaxRunFactor)
            {
                choice.Report.LengthRejected++;
                return;
            }

            RunBlockers blockers = Blockers(request, i, j);
            if (!blockers.Valid)
            {
                choice.Report.ClearanceRejected++;
                return;
            }
            bool noPlanet = request.RequirePlanetBlocker && !blockers.Planet;
            bool sunChord = !request.AllowSunOnChord && blockers.Sun;
            if (noPlanet)
                choice.Report.NoPlanetBlocker++;
            if (sunChord)
                choice.Report.SunOnChord++;
            if (noPlanet || sunChord)
                return;

            if (!blockers.Planet && !blockers.Sun)
            {
                choice.Report.NoBlocker++;
                return;
            }

            choice.Report.Accepted++;
            float delta = Mathf.Abs(run - request.TargetRun);
            if (delta >= choice.Best)
                return;

            choice.Best = delta;
            choice.Origin = from;
            choice.Destination = to;
            choice.Found = true;
        }

        private static RunBlockers Blockers(
            PuzzleRunRequest request, int a, int b)
        {
            Vector3 from = request.Planets[a].position;
            Vector3 to = request.Planets[b].position;
            if (!EndpointsClear(request, a, b))
                return default;

            bool sun = SunBlocked(request, from, to);
            bool planet = PlanetBlocked(request, a, b, from, to);
            return new RunBlockers(true, planet, sun);
        }

        private static int IndexOf(PuzzleFieldPlanet[] planets, Vector3 point)
        {
            for (int i = 0; i < planets.Length; i++)
                if ((planets[i].position - point).sqrMagnitude < 0.000001f)
                    return i;

            return -1;
        }

        private static bool EndpointsClear(
            PuzzleRunRequest request, int a, int b)
        {
            Vector3 from = request.Planets[a].position;
            Vector3 to = request.Planets[b].position;
            if (request.SunRadius > 0f
                && (Vector3.Distance(Vector3.zero, from) < request.EndpointClearance + request.SunRadius
                    || Vector3.Distance(Vector3.zero, to) < request.EndpointClearance + request.SunRadius))
                return false;

            for (int k = 0; k < request.Planets.Length; k++)
            {
                if (k == a || k == b)
                    continue;

                float radius = request.EndpointClearance + request.Planets[k].radius;
                if (Vector3.Distance(request.Planets[k].position, from) < radius
                    || Vector3.Distance(request.Planets[k].position, to) < radius)
                    return false;
            }

            return true;
        }

        private static bool SunBlocked(
            PuzzleRunRequest request, Vector3 from, Vector3 to) =>
            request.SunRadius > 0f
            && PuzzleRunGeometry.DistanceToSegment(Vector3.zero, from, to)
                < request.SunRadius * SunBlockBandFactor;

        private static bool PlanetBlocked(
            PuzzleRunRequest request, int a, int b, Vector3 from, Vector3 to)
        {
            for (int k = 0; k < request.Planets.Length; k++)
            {
                if (k == a || k == b)
                    continue;

                float radius = request.Planets[k].radius;
                if (PuzzleRunGeometry.DistanceToSegment(
                        request.Planets[k].position, from, to)
                    < radius * PlanetBlockBandFactor)
                    return true;
            }

            return false;
        }

        private readonly struct RunBlockers
        {
            public readonly bool Valid;
            public readonly bool Planet;
            public readonly bool Sun;

            public RunBlockers(bool valid, bool planet, bool sun)
            {
                Valid = valid;
                Planet = planet;
                Sun = sun;
            }
        }

        public readonly struct Candidate
        {
            public readonly Vector3 Origin;
            public readonly Vector3 Destination;
            public readonly float Delta;

            public Candidate(Vector3 origin, Vector3 destination, float delta)
            {
                Origin = origin;
                Destination = destination;
                Delta = delta;
            }
        }

        private struct RunChoice
        {
            public Vector3 Origin;
            public Vector3 Destination;
            public float Best;
            public bool Found;
            public PuzzleRunReport Report;
        }
    }
}
