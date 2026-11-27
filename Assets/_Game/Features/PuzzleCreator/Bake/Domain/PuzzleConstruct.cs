using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Builds a field around an intended answer: the chords first, then the lesson's planets, then the
    // decoys clear of the corridor, then the sun. Plain data all the way; the verdict is separate.
    public static class PuzzleConstruct
    {
        public static PuzzleConstructedField Build(PuzzleConstructRequest request)
        {
            Parts parts = Place(request);
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

            SeededRandom random = new(PuzzleConstructSeeds.Value(field.SeedValue, retry));
            return PuzzleConstructDecoys.TryRelocate(decoys, index, random)
                ? field.WithPlanet(index, planets[index].position)
                : null;
        }

        private static Parts Place(PuzzleConstructRequest request)
        {
            int seedValue = PuzzleConstructSeeds.Value(request.Seed.Seed, request.Attempt);
            PuzzleFieldPlanet[] planets = Planets(request, seedValue);
            if (planets == null)
                return null;

            Parts parts = request.Brief.IsMulti
                ? Fleet(planets, request, seedValue)
                : Single(planets, request, seedValue);
            if (parts == null || !AddLesson(parts) || !AddSun(parts))
                return null;

            AddCube(parts);
            return parts;
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
            if (!PuzzleConstructSun.ClearsChord(
                    sunHome, parts.Ships[0].Origin, parts.Ships[0].Destination, parts.Request))
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
            SectorShapeData shape = PuzzleFieldBuilder.Build(
                request.Seed.WithSeed(seedValue), request.Field);
            PuzzleFieldPlanet[] planets = PuzzleFieldGenerator.Generate(shape, seedValue);
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
