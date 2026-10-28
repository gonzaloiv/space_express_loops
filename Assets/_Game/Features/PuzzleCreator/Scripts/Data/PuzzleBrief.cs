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
        public Vector3 FixedZoneA;
        public Vector3 FixedZoneB;
        public float FixedZoneSize;

        public bool SupportsSingleZone => Moves <= 1 && !Rotate;

        public bool SupportsMoves =>
            Moves <= 3 && !Rotate && (Moves <= 1 || !OffPath);

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
            OffPath = seed.offPath,
            FixedZoneA = seed.fixedZoneA,
            FixedZoneB = seed.fixedZoneB,
            FixedZoneSize = Mathf.Max(seed.fixedZoneSize, 0.01f)
        };
    }
}
