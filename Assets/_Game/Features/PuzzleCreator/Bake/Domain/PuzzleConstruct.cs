using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Which plain step returned null: the scan reports it instead of a bare NoRun.
    public enum PuzzleConstructStep
    {
        None,
        Planets,
        Chords,
        Lesson,
        Sun
    }

    // Builds a field around an intended answer: the chords first, then the lesson's planets, then the
    // decoys clear of the corridor, then the sun. Plain data all the way; the verdict is separate.
    public static class PuzzleConstruct
    {
        public static PuzzleConstructedField Build(PuzzleConstructRequest request) =>
            Build(request, out _);

        public static PuzzleConstructedField Build(
            PuzzleConstructRequest request, out PuzzleConstructStep failure)
        {
            Parts parts = Place(request, out failure);
            return parts != null ? Field(parts) : null;
        }

        public static PuzzleConstructedField MoveDecoy(
            PuzzleConstructedField field, PuzzleConstructRequest request, int retry, int ship = -1)
        {
            PuzzleFieldPlanet[] planets = (PuzzleFieldPlanet[])field.Planets.Clone();
            PuzzleDecoyField decoys = Decoys(planets, field.Protected, request, field.Ships);
            int index = PuzzleConstructDecoys.NearestUnprotected(decoys, ship);
            if (index < 0)
                return null;

            int shapeSeed = PuzzleConstructSeeds.Value(field.SeedValue, retry);
            return PuzzleConstructDecoys.TryRelocate(decoys, index, shapeSeed)
                ? field.WithPlanet(index, planets[index].position)
                : null;
        }

        private static Parts Place(
            PuzzleConstructRequest request, out PuzzleConstructStep failure)
        {
            failure = PuzzleConstructStep.None;
            int seedValue = PuzzleConstructSeeds.Value(request.Seed.Seed, request.Attempt);
            PuzzleFieldPlanet[] planets = Planets(request, seedValue);
            if (planets == null)
                return Step(PuzzleConstructStep.Planets, out failure);

            Parts parts = request.Brief.IsMulti
                ? Fleet(planets, request, seedValue)
                : Single(planets, request, seedValue);
            if (parts == null)
                return Step(PuzzleConstructStep.Chords, out failure);

            if (!AddLesson(parts))
                return Step(PuzzleConstructStep.Lesson, out failure);

            if (!AddSun(parts))
                return Step(PuzzleConstructStep.Sun, out failure);

            AddCube(parts);
            return parts;
        }

        private static Parts Step(PuzzleConstructStep step, out PuzzleConstructStep failure)
        {
            failure = step;
            return null;
        }

        private static Parts Single(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int seedValue)
        {
            if (!PuzzleConstructChord.TryPick(
                    planets, request, out int origin, out int destination))
                return null;

            return new Parts
            {
                Planets = planets,
                Request = request,
                SeedValue = seedValue,
                Hub = -1,
                Ships = new[] { Ship(planets, origin, destination) }
            };
        }

        private static Parts Fleet(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int seedValue)
        {
            if (request.Brief.Relation == PuzzleShipRelation.FanOut)
            {
                PuzzleConstructPlacement placed =
                    PuzzleConstructFanOut.Build(planets, request, seedValue);
                if (placed == null)
                    return null;

                return new Parts
                {
                    Planets = placed.Planets,
                    Request = request,
                    SeedValue = seedValue,
                    Hub = placed.HubIndex,
                    HubHome = placed.HubHome,
                    Ships = placed.Ships
                };
            }

            if (!PuzzleConstructChord.TryPickShips(
                    planets, request, out PuzzleConstructedShip[] ships, out int hub,
                    out Vector3 hubHome))
                return null;

            return new Parts
            {
                Planets = planets,
                Request = request,
                SeedValue = seedValue,
                Hub = hub,
                HubHome = hubHome,
                Ships = ships
            };
        }

        private static PuzzleConstructedShip Ship(
            PuzzleFieldPlanet[] planets, int origin, int destination) => new()
        {
            OriginIndex = origin,
            DestinationIndex = destination,
            Origin = planets[origin].position,
            Destination = planets[destination].position,
            CaptureRadius = planets[destination].radius
        };

        private static bool AddLesson(Parts parts)
        {
            PuzzleLessonApplied lesson = PuzzleConstructLesson.Apply(
                parts.Planets, parts.Request, parts.Ships, parts.Hub, parts.HubHome);
            if (lesson == null)
                return false;

            parts.Lesson = lesson;
            PuzzleConstructDecoys.KeepClear(Decoys(
                parts.Planets, lesson.Protected, parts.Request, parts.Ships));
            return true;
        }

        private static bool AddSun(Parts parts)
        {
            if (!PuzzleConstructSun.TryPlace(
                    parts.Planets, parts.Request, out Vector3 sunHome, out bool hasSun))
                return false;
            if (!PuzzleConstructSun.ClearsChords(sunHome, parts.Ships, parts.Request))
                return false;

            parts.SunHome = sunHome;
            parts.HasSun = hasSun;
            return true;
        }

        private static void AddCube(Parts parts) =>
            PuzzleFieldBake.CubeRange(parts.Planets, out parts.CubeMin, out parts.CubeMax);

        private static PuzzleFieldPlanet[] Planets(
            PuzzleConstructRequest request, int seedValue)
        {
            PuzzleFieldPlanet[] planets = PuzzleConstructShape.Generate(request, seedValue);
            return planets.Length >= 2 ? planets : null;
        }

        private static PuzzleConstructedField Field(Parts parts) => new()
        {
            Planets = parts.Planets,
            Origin = parts.Ships[0].Origin,
            Destination = parts.Ships[0].Destination,
            OriginIndex = parts.Ships[0].OriginIndex,
            DestinationIndex = parts.Ships[0].DestinationIndex,
            Ships = parts.Ships,
            HubIndex = parts.Hub,
            HubHome = parts.HubHome,
            Protected = parts.Lesson.Protected,
            HasSun = parts.HasSun,
            SunHome = parts.SunHome,
            SunRadius = parts.Request.Brief.SunRadius,
            SunScalable = parts.Request.Brief.SunScalable,
            CubeSizeMin = parts.CubeMin,
            CubeSizeMax = parts.CubeMax,
            Answer = parts.Lesson.Plan.Answer,
            Kind = parts.Lesson.Plan.Kind,
            SeedValue = parts.SeedValue,
            Attempt = parts.Request.Attempt
        };

        private static PuzzleDecoyField Decoys(
            PuzzleFieldPlanet[] planets, bool[] isProtected, PuzzleConstructRequest request,
            PuzzleConstructedShip[] ships) => new()
        {
            Planets = planets,
            Protected = isProtected,
            Request = request,
            Origin = ships[0].Origin,
            Destination = ships[0].Destination,
            Ships = ships
        };

        private sealed class Parts
        {
            public PuzzleFieldPlanet[] Planets;
            public PuzzleConstructRequest Request;
            public PuzzleConstructedShip[] Ships;
            public int Hub;
            public Vector3 HubHome;
            public int SeedValue;
            public PuzzleLessonApplied Lesson;
            public Vector3 SunHome;
            public bool HasSun;
            public float CubeMin;
            public float CubeMax;
        }
    }
}
