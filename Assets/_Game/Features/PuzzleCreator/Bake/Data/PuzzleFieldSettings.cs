using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The shape settings the field builder reads, copied out of the asset so the constructor's steps
    // do not touch a ScriptableObject.
    public struct PuzzleFieldSettings
    {
        public float RunDistance;
        public float PlanetRadiusMin;
        public float PlanetRadiusMax;
        public float SeparationScale;
        public float FieldThickness;
        public float FieldTilt;
        public float FieldBulge;

        public static PuzzleFieldSettings From(SpaceShapingSettings settings) => new()
        {
            RunDistance = settings.runDistance,
            PlanetRadiusMin = settings.planetRadiusMin,
            PlanetRadiusMax = settings.planetRadiusMax,
            SeparationScale = settings.separationScale,
            FieldThickness = settings.fieldThickness,
            FieldTilt = settings.fieldTilt,
            FieldBulge = settings.fieldBulge
        };
    }
}
