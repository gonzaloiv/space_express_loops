using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleJobContract
    {
        public string Id;
        public bool Pending;
        public int Seed;
        public int Attempts;
        public int PlanetCount;
        public PuzzleFieldShape FieldShape;
        public bool HasSun;
        public PuzzleSunPlacement SunPlacement;
        public bool SunOnChord;
        public PuzzleSunRule SunRule;
        public int MinBlockers;
        public PuzzleMechanism Mechanism;
        public int Zones;
        public int MinRoutes;
        public int Ships;
        public PuzzleShipRelation Relation;
        public bool Translatable;
        public PuzzleStartRule StartRule;
        public bool NoFail;
        public bool NoGrade;
        public bool NoIntro;
        public bool GrowOnly;

        // Scramble bounds, each 0 = off. Low scramble: the solution's |scale - 1| stays at most
        // MaxScaleDelta. High scramble: it is at least MinScaleDelta, and on a movable job the cube
        // starts at least MinStartDistance from the solution zone. Checked against the stored answer,
        // both at bake time and in ShippedJobs_MeetTheContract.
        public float MaxScaleDelta;
        public float MinScaleDelta;
        public float MinStartDistance;

        // The one contract-to-seed mapping: the bake tests, the seed scan and the test set all bake
        // from this, so a field added to the contract cannot be dropped by one of them.
        public PuzzleSeed CreateSeed()
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.seed = Seed;
            seed.planetCount = PlanetCount;
            seed.fieldShape = FieldShape;
            seed.hasSun = HasSun;
            seed.sunPlacement = SunPlacement;
            seed.sunRule = SunRule;
            seed.sunOnChord = SunOnChord;
            seed.mechanism = Mechanism;
            seed.zones = Zones;
            seed.minRoutes = MinRoutes;
            seed.ships = Mathf.Max(1, Ships);
            seed.relation = Relation;
            seed.translatable = Translatable;
            seed.startRule = StartRule;
            seed.noFail = NoFail;
            seed.noGrade = NoGrade;
            seed.noIntro = NoIntro;
            seed.growOnly = GrowOnly;
            seed.resizable = false;
            return seed;
        }
    }
}
