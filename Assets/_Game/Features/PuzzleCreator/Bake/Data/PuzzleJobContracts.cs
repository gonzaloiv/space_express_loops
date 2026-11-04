using System.Collections.Generic;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleJobContracts
    {
        public static readonly IReadOnlyList<PuzzleJobContract> All = new[]
        {
            new PuzzleJobContract
            {
                Id = "01_01", Seed = 1, Attempts = 200, PlanetCount = 4, FieldShape = PuzzleFieldShape.Ring,
                HasSun = false, SunOnChord = false, SunRule = PuzzleSunRule.Any, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
                Translatable = false, NoFail = true, NoGrade = true,
            },
            new PuzzleJobContract
            {
                Id = "01_02", Seed = 2, Attempts = 200, PlanetCount = 6, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.CentredGrown, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
                Translatable = false, NoFail = false, NoGrade = true,
            },
            new PuzzleJobContract
            {
                Id = "01_03", Seed = 2, Attempts = 200, PlanetCount = 6, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 2,
                Translatable = false, NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_04", Seed = 7, Attempts = 12, PlanetCount = 8, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Mixed, Zones = 2, MinRoutes = 2,
                Translatable = false, NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_05", Seed = 2, Attempts = 8, PlanetCount = 8, FieldShape = PuzzleFieldShape.Spiral,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 2,
                Translatable = true, StartRule = PuzzleStartRule.FieldCentre,
                NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_06", Seed = 1, Attempts = 12, PlanetCount = 10, FieldShape = PuzzleFieldShape.Spiral,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 2, MinRoutes = 1,
                Translatable = true, NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_07", Seed = 8, Attempts = 12, PlanetCount = 10, FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 2, MinRoutes = 2,
                Translatable = true, NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_08", Seed = 2, Attempts = 12, PlanetCount = 12, FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.InZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Mixed, Zones = 2, MinRoutes = 2,
                Translatable = true, StartRule = PuzzleStartRule.FieldCentre,
                NoFail = false, NoGrade = false,
            },
        };
    }
}
