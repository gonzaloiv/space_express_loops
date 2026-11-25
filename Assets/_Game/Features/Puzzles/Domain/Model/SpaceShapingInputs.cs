using DigitalLove.Game.Spatial;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The plain values a setup needs, so a caller holding a snapshot of the field (a bake worker, a
    // tool) builds the same SpaceShapingSetup without a PuzzleData asset.
    public struct SpaceShapingInputs
    {
        public Vector3 Origin;
        public Vector3 Destination;
        public SourceSpec[] Sources;
        public ShipSpec[] Ships;
        public float CaptureRadius;
        public float EntropyBudget;
        public bool NoFail;
        public bool NoGrade;
        public PuzzleSimSettings Settings;
    }
}
