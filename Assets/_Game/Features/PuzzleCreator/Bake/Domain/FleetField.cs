using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A plain copy of the field values the multi-ship search reads, so the probe can run off the
    // editor thread: PuzzleData is a ScriptableObject and must not travel with the work. The copy is
    // made once, on the main thread, at bake start.
    public sealed class FleetField
    {
        public readonly PuzzleSource[] Sources;
        public readonly PuzzleShip[] Ships;
        public readonly Vector3 Origin;
        public readonly Vector3 Destination;
        public readonly float CaptureRadius;
        public readonly float EntropyBudget;
        public readonly float CubeSizeMin;
        public readonly float CubeSizeMax;
        public readonly bool NoFail;
        public readonly bool NoGrade;
        public readonly PuzzleSimSettings Settings;

        private FleetField(PuzzleData puzzle, PuzzleSimSettings settings)
        {
            Sources = Copy(puzzle.sources);
            Ships = Copy(puzzle.ships);
            Origin = puzzle.Origin;
            Destination = puzzle.Destination;
            CaptureRadius = puzzle.EffectiveCaptureRadius;
            EntropyBudget = puzzle.entropyBudget;
            CubeSizeMin = puzzle.cubeSizeMin;
            CubeSizeMax = puzzle.cubeSizeMax;
            NoFail = puzzle.noFail;
            NoGrade = puzzle.noGrade;
            Settings = settings;
        }

        public static FleetField From(PuzzleData puzzle, PuzzleSimSettings settings) =>
            new(puzzle, settings);

        public SpaceShapingSetup ToSetup() =>
            SpaceShapingSetup.Build(new SpaceShapingInputs
            {
                Origin = Origin,
                Destination = Destination,
                Sources = SourceSpecs.From(Sources, Settings),
                Ships = ShipSpecs.From(Ships, Origin, Destination),
                CaptureRadius = CaptureRadius,
                EntropyBudget = EntropyBudget,
                NoFail = NoFail,
                NoGrade = NoGrade,
                Settings = Settings
            });

        // The field's capture radius at a point, the same rule PuzzleData uses to pick a ship's.
        public float CaptureRadiusAt(Vector3 point)
        {
            for (int i = 0; i < Sources.Length; i++)
            {
                if ((Sources[i].home - point).sqrMagnitude < 0.000001f)
                    return Sources[i].scale * 0.5f;
            }

            return 0f;
        }

        private static T[] Copy<T>(T[] values) =>
            values == null ? System.Array.Empty<T>() : (T[])values.Clone();
    }
}
