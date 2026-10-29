using DigitalLove.Game.Sectors;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PuzzleShape", menuName = "DigitalLove/Game/Puzzle Shape")]
    public class PuzzleShapeAsset : ScriptableObject
    {
        public int seed = 1;
        public SectorShapeData shape = new();
    }
}
