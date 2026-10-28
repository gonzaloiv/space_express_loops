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
        public float StartMargin;
        public bool OffPath;

        public bool SupportsSingleZone => Moves <= 1 && !Rotate;

        public static PuzzleBrief From(PuzzleSeed seed, SpaceShapingSettings settings) => new()
        {
            MinCubeSize = settings.cubeSizeMin,
            MaxCubeSize = settings.cubeSizeMax,
            CorridorClearance = settings.corridorClearance,
            WindowTightness = Mathf.Clamp01(seed.windowTightness),
            Translatable = seed.translatable,
            Moves = Mathf.Max(1, seed.moves),
            Resizable = seed.resizable,
            Rotate = seed.rotate,
            StartMargin = Mathf.Clamp(seed.startMargin, 0.1f, 2f),
            OffPath = seed.offPath
        };
    }
}
