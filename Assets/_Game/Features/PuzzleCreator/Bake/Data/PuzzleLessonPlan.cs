using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // What a lesson may do to the generated field: move a decoy's body onto the intended answer, and
    // name the answer. A move never adds or removes a planet, so PlanetCount stays the contract's.
    public readonly struct PuzzleLessonMove
    {
        public readonly int Index;
        public readonly Vector3 Home;
        public readonly float Radius;

        public PuzzleLessonMove(int index, Vector3 home, float radius)
        {
            Index = index;
            Home = home;
            Radius = radius;
        }
    }

    public sealed class PuzzleLessonContext
    {
        public PuzzleFieldPlanet[] Planets;
        public int OriginIndex;
        public int DestinationIndex;
        public Vector3 Origin;
        public Vector3 Destination;
        public float CubeSizeMin;
        public float CubeSizeMax;
        public float SunRadius;
        public PuzzleSimSettings Settings;
        public int SeedValue;
    }

    public sealed class PuzzleLessonPlan
    {
        public readonly PuzzleLessonMove[] Moves;
        public readonly PuzzleZone[] Answer;
        public readonly PuzzleSolutionKind Kind;

        public PuzzleLessonPlan(
            PuzzleLessonMove[] moves, PuzzleZone[] answer, PuzzleSolutionKind kind)
        {
            Moves = moves;
            Answer = answer;
            Kind = kind;
        }

        public static PuzzleLessonPlan None => new(
            System.Array.Empty<PuzzleLessonMove>(), System.Array.Empty<PuzzleZone>(),
            PuzzleSolutionKind.Other);
    }

    // The constructor's seam for the Clear and Curve recipes (and, later, the fleet recipes).
    public interface IPuzzleLesson
    {
        PuzzleLessonPlan Place(PuzzleLessonContext context);
    }

    public sealed class PuzzleLessonApplied
    {
        public PuzzleLessonPlan Plan;
        public bool[] Protected;
    }
}
