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
        // Endpoints and lesson planets: the decoy step never moves these.
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
