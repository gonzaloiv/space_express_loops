using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleBrief
    {
        public float MinCubeSize;
        public float MaxCubeSize;
        public float CorridorClearance;
        public float WindowTightness;
        public bool Translatable;
        public int Moves;
        public bool Resizable;
        public bool Rotate;

        public bool SupportsSingleZone => Moves <= 1 && !Rotate;

        public static PuzzleBrief From(PuzzleSeed seed) => new()
        {
            MinCubeSize = seed.minCubeSize,
            MaxCubeSize = seed.maxCubeSize,
            CorridorClearance = seed.corridorClearance,
            WindowTightness = Mathf.Clamp01(seed.windowTightness),
            Translatable = seed.translatable,
            Moves = Mathf.Max(1, seed.moves),
            Resizable = seed.resizable,
            Rotate = seed.rotate
        };
    }
}
