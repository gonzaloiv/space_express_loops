using System.Collections.Generic;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleJobContracts
    {
        public static readonly IReadOnlyList<PuzzleJobContract> All = new[]
        {
            new PuzzleJobContract
            {
                Id = "01_01", Seed = 141, Attempts = 1, PlanetCount = 6, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
                Translatable = false, NoFail = true, NoGrade = true, NoIntro = true,
            },
            new PuzzleJobContract
            {
                Id = "01_02", Seed = 20, Attempts = 1, PlanetCount = 6, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.CentredGrown, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
                Translatable = false, NoFail = false, NoGrade = true,
            },
            new PuzzleJobContract
            {
                Id = "01_03", Seed = 146, Attempts = 1, PlanetCount = 6, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 2,
                Translatable = false, NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_04", Seed = 16, Attempts = 1, PlanetCount = 8, FieldShape = PuzzleFieldShape.Spiral,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 2,
                Translatable = true, StartRule = PuzzleStartRule.FieldCentre,
                NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_05", Seed = 124, Attempts = 1, PlanetCount = 10, FieldShape = PuzzleFieldShape.Spiral,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 2, MinRoutes = 1,
                Translatable = true, NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_06", Seed = 19, Attempts = 1, PlanetCount = 10, FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 2, MinRoutes = 2,
                Translatable = true, NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_07", Seed = 26, Attempts = 1, PlanetCount = 12, FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.InZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 2, MinRoutes = 2,
                Translatable = true, StartRule = PuzzleStartRule.FieldCentre,
                NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_08", Pending = true,
                Seed = 2, Attempts = 200, PlanetCount = 12, FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.InZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Mixed, Zones = 2, MinRoutes = 2,
                Translatable = true, StartRule = PuzzleStartRule.FieldCentre,
                NoFail = false, NoGrade = false,
            },
        };

        public static readonly IReadOnlyList<PuzzleJobContract> Enabled = EnabledJobs();

        private static IReadOnlyList<PuzzleJobContract> EnabledJobs()
        {
            List<PuzzleJobContract> enabled = new List<PuzzleJobContract>();
            for (int i = 0; i < All.Count; i++)
            {
                if (!All[i].Pending)
                    enabled.Add(All[i]);
            }

            return enabled;
        }

        public static PuzzleJobContract ById(string id)
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Id == id)
                    return All[i];
            }

            throw new System.ArgumentException($"Unknown job id {id}");
        }
    }
}
