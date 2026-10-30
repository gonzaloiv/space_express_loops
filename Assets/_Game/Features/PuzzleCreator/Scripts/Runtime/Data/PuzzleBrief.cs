using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleBrief
    {
        public float EndpointClearance;
        public bool Translatable;
        public int Moves;
        public bool Resizable;

        public bool SupportsMoves =>
            Moves <= 3;

        public static PuzzleBrief From(PuzzleSeed seed, SpaceShapingSettings settings) => new()
        {
            EndpointClearance = settings.endpointClearance,
            Translatable = seed.translatable,
            Moves = Mathf.Max(1, seed.moves),
            Resizable = seed.resizable
        };
    }
}
