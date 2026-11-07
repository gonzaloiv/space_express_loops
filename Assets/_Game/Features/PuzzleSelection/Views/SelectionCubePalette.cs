using System;
using DigitalLove.Global;

namespace DigitalLove.Game.PuzzleSelection
{
    // The map's per-state node colours (current, solved, locked).
    [Serializable]
    public struct SelectionCubePalette
    {
        public ColorValue Current;
        public ColorValue Solved;
        public ColorValue Locked;
    }
}
