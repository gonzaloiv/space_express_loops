using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Builds a field around an intended answer: the chord first, then the lesson's planets, then the
    // decoys clear of the corridor, then the sun. Plain data all the way; the verdict is separate.
    public static class PuzzleConstruct
    {
        public static PuzzleConstructedField Build(PuzzleConstructRequest request)
        {
            Parts parts = Place(request);
            return parts != null ? Field(parts) : null;
        }

        public static PuzzleConstructedField MoveDecoy(
            PuzzleConstructedField field, PuzzleConstructRequest request, int retry)
        {
            PuzzleFieldPlanet[] planets = (PuzzleFieldPlanet[])field.Planets.Clone();
            PuzzleDecoyField decoys = Decoys(
                planets, field.Protected, request, field.Origin, field.Destination);
            int index = PuzzleConstructDecoys.NearestUnprotected(decoys);
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
            if (planets == null
                || !PuzzleConstructChord.TryPick(
                    planets, request, out int origin, out int destination))
                return null;

            Parts parts = new()
            {
                Planets = planets,
                Request = request,
                Origin = origin,
                Destination = destination,
                SeedValue = seedValue
            };
            if (!AddLesson(parts) || !AddSun(parts))
                return null;

            AddCube(parts);
            return parts;
        }

        private static bool AddLesson(Parts parts)
        {
            PuzzleLessonApplied lesson = PuzzleConstructLesson.Apply(
                parts.Planets, parts.Request, parts.Origin, parts.Destination);
            if (lesson == null)
                return false;

            parts.Lesson = lesson;
            PuzzleConstructDecoys.KeepClear(Decoys(
                parts.Planets, lesson.Protected, parts.Request,
                parts.Planets[parts.Origin].position, parts.Planets[parts.Destination].position));
            return true;
        }

        private static bool AddSun(Parts parts)
        {
            if (!PuzzleConstructSun.TryPlace(
                    parts.Planets, parts.Request, out Vector3 sunHome, out bool hasSun))
                return false;
            if (!PuzzleConstructSun.ClearsChord(
                    sunHome, parts.Planets[parts.Origin].position,
                    parts.Planets[parts.Destination].position, parts.Request))
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
            Origin = parts.Planets[parts.Origin].position,
            Destination = parts.Planets[parts.Destination].position,
            OriginIndex = parts.Origin,
            DestinationIndex = parts.Destination,
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
            Vector3 origin, Vector3 destination) => new()
        {
            Planets = planets,
            Protected = isProtected,
            Request = request,
            Origin = origin,
            Destination = destination
        };

        private sealed class Parts
        {
            public PuzzleFieldPlanet[] Planets;
            public PuzzleConstructRequest Request;
            public int Origin;
            public int Destination;
            public int SeedValue;
            public PuzzleLessonApplied Lesson;
            public Vector3 SunHome;
            public bool HasSun;
            public float CubeMin;
            public float CubeMax;
        }
    }
}
