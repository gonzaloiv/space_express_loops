using System.Collections.Generic;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Pending construct copies for the wire comparison and the fleet plan: bake.ps1 writes them under
    // the creator's Scratch folder, never the shipped chapter. Pending keeps them out of Enabled, so
    // the chapter, the job window and the shipped checks skip them.
    public static class PuzzleConstructScratch
    {
        public const string Folder = "Assets/_Game/Features/PuzzleCreator/Scratch";

        public static readonly IReadOnlyList<PuzzleJobContract> All = new[]
        {
            new PuzzleJobContract
            {
                Id = "01_01C", Pending = true, Generator = PuzzleGenerator.Construct,
                Seed = 141, Attempts = 4, PlanetCount = 6, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
                Translatable = false, NoFail = true, NoGrade = true, NoIntro = true, GrowOnly = true,
            },
            new PuzzleJobContract
            {
                Id = "01_03C", Pending = true, Generator = PuzzleGenerator.Construct,
                Seed = 200, Attempts = 4, PlanetCount = 6, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.OutOfZone, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 2,
                Translatable = false, NoFail = false, NoGrade = false,
            },
            // Fleet Clear: 02_07's field, two and three ships. 12 planets for the third pair.
            new PuzzleJobContract
            {
                Id = "02_07C", Pending = true, Generator = PuzzleGenerator.Construct,
                Seed = 19, Attempts = 4, PlanetCount = 10, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.Separate,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
            new PuzzleJobContract
            {
                Id = "02_07C3", Pending = true, Generator = PuzzleGenerator.Construct,
                Seed = 19, Attempts = 4, PlanetCount = 12, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Clear, Zones = 1, MinRoutes = 1, Ships = 3,
                Relation = PuzzleShipRelation.Separate,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
            // Fleet Curve: 02_03's two-ship field with the role pinned to Curve, and 02_06's three.
            new PuzzleJobContract
            {
                Id = "02_03C", Pending = true, Generator = PuzzleGenerator.Construct,
                Seed = 23, Attempts = 4, PlanetCount = 10, FieldShape = PuzzleFieldShape.Ring,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1, Ships = 2,
                Relation = PuzzleShipRelation.Separate,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
            new PuzzleJobContract
            {
                Id = "02_06C", Pending = true, Generator = PuzzleGenerator.Construct,
                Seed = 86, Attempts = 4, PlanetCount = 14, FieldShape = PuzzleFieldShape.Cluster,
                HasSun = true, SunPlacement = PuzzleSunPlacement.OffCorridor, SunOnChord = false,
                SunRule = PuzzleSunRule.NoSunAnswer,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1, Ships = 3,
                Relation = PuzzleShipRelation.Separate,
                Translatable = true, NoFail = false, NoGrade = false, NoIntro = false,
            },
        };

        public static bool Has(string id)
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Id == id)
                    return true;
            }

            return false;
        }

        public static bool TryGet(string id, out PuzzleJobContract contract)
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Id != id)
                    continue;

                contract = All[i];
                return true;
            }

            contract = default;
            return false;
        }
    }
}
