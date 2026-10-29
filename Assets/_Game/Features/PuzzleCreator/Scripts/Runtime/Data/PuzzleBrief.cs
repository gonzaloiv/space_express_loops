using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleBrief
    {
        public float CorridorClearance;
        public bool Translatable;
        public int Moves;
        public bool Resizable;

        public bool SupportsMoves =>
            Moves <= 3 && (Moves <= 1 || Translatable);

        public static PuzzleBrief From(PuzzleSeed seed, SpaceShapingSettings settings) => new()
        {
            CorridorClearance = settings.corridorClearance,
            Translatable = seed.translatable,
            Moves = Mathf.Max(1, seed.moves),
            Resizable = seed.resizable
        };
    }
}
