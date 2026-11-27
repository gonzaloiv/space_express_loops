using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A field built around an intended answer, before it becomes a PuzzleData. Plain data only: the
    // verdict materializes and judges it on the main thread.
    public sealed class PuzzleConstructedField
    {
        public PuzzleFieldPlanet[] Planets;
        public Vector3 Origin;
        public Vector3 Destination;
        public int OriginIndex;
        public int DestinationIndex;
        // Every ship's pair; one ship for a single-ship job. Origin/Destination above are ship 0's.
        public PuzzleConstructedShip[] Ships;
        // The shared blocker/bend's planet, or -1 for a single-ship job.
        public int HubIndex;
        // The point the shared planet is moved to (the chords' midpoint centre).
        public Vector3 HubHome;
        // Endpoints, the hub and lesson planets: the decoy step never moves these.
        public bool[] Protected;
        public bool HasSun;
        public Vector3 SunHome;
        public float SunRadius;
        public bool SunScalable;
        public float CubeSizeMin;
        public float CubeSizeMax;
        public PuzzleZone[] Answer;
        public PuzzleSolutionKind Kind;
        public int SeedValue;
        public int Attempt;

        public PuzzleConstructedField WithPlanet(int index, Vector3 home)
        {
            PuzzleConstructedField copy = (PuzzleConstructedField)MemberwiseClone();
            copy.Planets = (PuzzleFieldPlanet[])Planets.Clone();
            copy.Planets[index].position = home;
            return copy;
        }
    }
}
