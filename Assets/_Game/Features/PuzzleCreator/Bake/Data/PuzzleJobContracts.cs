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
                Translatable = false, NoFail = true, NoGrade = true, NoIntro = true, GrowOnly = true,
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
                Id = "01_03", Seed = 200, Attempts = 1, PlanetCount = 6, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 2,
                Translatable = false, NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_04", Seed = 79, Attempts = 1, PlanetCount = 8, FieldShape = PuzzleFieldShape.Spiral,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 2,
                Translatable = true, StartRule = PuzzleStartRule.FieldCentre,
                NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "01_05", Seed = 140, Attempts = 1, PlanetCount = 10, FieldShape = PuzzleFieldShape.Spiral,
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
                Id = "01_07", Seed = 65, Attempts = 1, PlanetCount = 12, FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.InZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 2, MinRoutes = 2,
                Translatable = true, StartRule = PuzzleStartRule.FieldCentre,
                NoFail = false, NoGrade = false,
            },
            new PuzzleJobContract
            {
                Id = "02_01", Seed = 23, Attempts = 1, PlanetCount = 10, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Any, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.Separate,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
            new PuzzleJobContract
            {
                Id = "02_02", Seed = 4, Attempts = 1, PlanetCount = 12, FieldShape = PuzzleFieldShape.Spiral,
                HasSun = true, SunPlacement = PuzzleSunPlacement.Centre, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 0,
                Mechanism = PuzzleMechanism.Any, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.FanOut,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
            new PuzzleJobContract
            {
                Id = "02_03", Seed = 8, Attempts = 1, PlanetCount = 12, FieldShape = PuzzleFieldShape.Spiral,
                HasSun = true, SunPlacement = PuzzleSunPlacement.Centre, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 0,
                Mechanism = PuzzleMechanism.Any, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.Converge,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
            // Fleet capstone: three ships from one origin, the chapter's hardest field. Seed 9 is the
            // widest of two passing seeds in the 2026-11-25 scan (window 0.19); its answer is a 0.40
            // scale delta at a start 0.183 away, so the high-scramble bounds sit just under both. The
            // headset pass confirms them.
            new PuzzleJobContract
            {
                Id = "02_05", Seed = 9, Attempts = 1, PlanetCount = 12,
                FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer,
                Mechanism = PuzzleMechanism.Any, Zones = 1, MinRoutes = 1, Ships = 3,
                Relation = PuzzleShipRelation.FanOut,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
                MinScaleDelta = 0.35f, MinStartDistance = 0.15f,
            },
            // Chapter 2 intro: two fixed-cube jobs, baked 2026-11-25. MaxScaleDelta is the widest
            // baked answer's |scale - 1|; tune it by playing.
            new PuzzleJobContract
            {
                Id = "02_09", Seed = 7, Attempts = 1, PlanetCount = 9,
                FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer,
                Mechanism = PuzzleMechanism.Any, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.FanOut,
                Translatable = false, NoFail = false, NoGrade = false, NoIntro = false,
                MaxScaleDelta = 0.5f,
            },
            new PuzzleJobContract
            {
                Id = "02_10", Seed = 5, Attempts = 1, PlanetCount = 9,
                FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer,
                Mechanism = PuzzleMechanism.Any, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.Separate,
                Translatable = false, NoFail = false, NoGrade = false, NoIntro = false,
                MaxScaleDelta = 0.5f,
            },
            // Fleet Clear: movable two-ship jobs whose lesson is one blocker shared by both runs.
            // Pending until the off-editor Clear scan pins Seed/Attempts and the scramble bounds are
            // set by playing; then unpin and bake them into chapter 2 after 02_04.
            new PuzzleJobContract
            {
                Id = "02_06", Pending = true, Seed = 1, Attempts = 1, PlanetCount = 10,
                FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.Separate,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
            new PuzzleJobContract
            {
                Id = "02_07", Pending = true, Seed = 1, Attempts = 1, PlanetCount = 12,
                FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.Separate,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
            new PuzzleJobContract
            {
                Id = "02_08", Pending = true, Seed = 1, Attempts = 1, PlanetCount = 12,
                FieldShape = PuzzleFieldShape.Spiral,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 2,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.Separate,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
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

        public static string ChapterOf(string id)
        {
            int index = id != null ? id.IndexOf('_') : -1;
            return index < 0 ? id : id.Substring(0, index);
        }
    }
}
