using System.Collections.Generic;
using DigitalLove.Global;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class RouteEndpointsTests
    {
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                if (created[i] != null)
                    Object.DestroyImmediate(created[i]);
            }

            created.Clear();
        }

        [Test]
        public void Build_SingleShipHasOneOriginAndOneDestination()
        {
            PuzzleData puzzle = SingleShip();
            RouteEndpoints endpoints = endpointsFor(puzzle);

            Assert.AreEqual(2, endpoints.Count);
            Assert.AreEqual(RouteEndpointRole.Origin, endpoints.RoleAt(0));
            Assert.AreEqual(RouteEndpointRole.Destination, endpoints.RoleAt(1));
            Assert.AreEqual(1, endpoints.RouteCount(0));
            Assert.AreEqual(1, endpoints.RouteCount(1));
        }

        [Test]
        public void Build_SeparateShipsHaveTwoEndpointsEach()
        {
            PuzzleData puzzle = TwoShips(
                new Vector3(-4f, 2f, 0f), new Vector3(4f, 2f, 0f));
            RouteEndpoints endpoints = endpointsFor(puzzle);

            Assert.AreEqual(4, endpoints.Count);
            Assert.AreEqual(2, CountEndpoints(endpoints, RouteEndpointRole.Origin));
            Assert.AreEqual(2, CountEndpoints(endpoints, RouteEndpointRole.Destination));
            Assert.AreEqual(0, CountEndpoints(endpoints, RouteEndpointRole.Interchange));
        }

        [Test]
        public void Build_FanOutSharesOneInterchangeWithTwoPips()
        {
            PuzzleData puzzle = TwoShips(
                new Vector3(-4f, 0f, 0f), new Vector3(4f, 2f, 0f));
            RouteEndpoints endpoints = endpointsFor(puzzle);

            Assert.AreEqual(3, endpoints.Count);
            int shared = Entry(endpoints, new Vector3(-4f, 0f, 0f));
            Assert.AreEqual(RouteEndpointRole.Interchange, endpoints.RoleAt(shared));
            Assert.AreEqual(2, endpoints.RouteCount(shared));
            Assert.AreEqual(2, endpoints.PipCount(shared));
            Assert.AreEqual(Color.red, endpoints.ColorAt(shared, 0));
            Assert.AreEqual(Color.green, endpoints.ColorAt(shared, 1));
        }

        [Test]
        public void Build_ThreeRouteInterchangeHasThreePips()
        {
            PuzzleData puzzle = ThreeShipsSharingOrigin();
            RouteEndpoints endpoints = endpointsFor(puzzle);

            int shared = Entry(endpoints, new Vector3(-4f, 0f, 0f));
            Assert.AreEqual(RouteEndpointRole.Interchange, endpoints.RoleAt(shared));
            Assert.AreEqual(3, endpoints.PipCount(shared));
            Assert.AreEqual(Color.blue, endpoints.ColorAt(shared, 2));
        }

        [Test]
        public void Build_OriginOfOneShipAndDestinationOfAnotherIsInterchange()
        {
            PuzzleData puzzle = TwoShips(
                new Vector3(4f, 0f, 0f), new Vector3(4f, 2f, 0f));
            RouteEndpoints endpoints = endpointsFor(puzzle);

            int shared = Entry(endpoints, new Vector3(4f, 0f, 0f));
            Assert.AreEqual(RouteEndpointRole.Interchange, endpoints.RoleAt(shared));
            Assert.AreEqual(2, endpoints.PipCount(shared));
        }

        [Test]
        public void Resolve_PrimaryDestinationOnConvergeIsTheSharedSource()
        {
            PuzzleData puzzle = TwoShips(
                new Vector3(-4f, 2f, 0f), new Vector3(4f, 0f, 0f));
            RouteEndpoints endpoints = endpointsFor(puzzle);

            int shared = endpoints.PrimaryDestination;
            Assert.AreNotEqual(-1, shared);
            Assert.AreEqual(RouteEndpointRole.Interchange, endpoints.RoleAt(shared));
            Assert.AreEqual(sourcesAt(puzzle, new Vector3(4f, 0f, 0f)), endpoints.SourceAt(shared));
            Assert.AreEqual(endpoints.SourceAt(shared), endpoints.PrimaryDestinationSource);
        }

        [Test]
        public void Resolve_EndpointWithNoSourceStaysUnresolved()
        {
            PuzzleData puzzle = TwoShips(
                new Vector3(-4f, 2f, 0f), new Vector3(4f, 2f, 0f));
            puzzle.sources = new[] { Source(new Vector3(-4f, 0f, 0f)) };
            RouteEndpoints endpoints = endpointsFor(puzzle);

            Assert.AreEqual(-1, endpoints.SourceAt(Entry(endpoints, new Vector3(4f, 0f, 0f))));
            Assert.AreEqual(0, endpoints.SourceAt(Entry(endpoints, new Vector3(-4f, 0f, 0f))));
            Assert.IsTrue(endpoints.IsEndpointSource(0));
            Assert.IsFalse(endpoints.IsEndpointSource(1));
        }

        private RouteEndpoints endpointsFor(PuzzleData puzzle)
        {
            RouteEndpoints endpoints = new();
            endpoints.Build(RoutePlan.Build(puzzle, Palette()));
            endpoints.Resolve(new SpaceShapingSession(
                SpaceShapingSetup.Build(puzzle, Sim())));
            return endpoints;
        }

        private static int CountEndpoints(RouteEndpoints endpoints, RouteEndpointRole role)
        {
            int count = 0;
            for (int i = 0; i < endpoints.Count; i++)
            {
                if (endpoints.RoleAt(i) == role)
                    count++;
            }

            return count;
        }

        private static int Entry(RouteEndpoints endpoints, Vector3 point)
        {
            for (int i = 0; i < endpoints.Count; i++)
            {
                if (endpoints.PointAt(i) == point)
                    return i;
            }

            return -1;
        }

        private static int sourcesAt(PuzzleData puzzle, Vector3 point)
        {
            for (int i = 0; i < puzzle.sources.Length; i++)
            {
                if (puzzle.sources[i].home == point)
                    return i;
            }

            return -1;
        }

        private ColorValue[] Palette()
        {
            Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };
            ColorValue[] palette = new ColorValue[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                palette[i] = Track(ScriptableObject.CreateInstance<ColorValue>());
                palette[i].value = colors[i];
            }

            return palette;
        }

        private static PuzzleSimSettings Sim() => new()
        {
            NominalSpeed = 1f,
            StepSecs = 1f / 120f,
            CaptureRadius = 0.6f
        };

        private PuzzleData SingleShip()
        {
            PuzzleData puzzle = Puzzle();
            puzzle.origin = new Vector3(-4f, 0f, 0f);
            puzzle.destination = new Vector3(4f, 0f, 0f);
            puzzle.ships = System.Array.Empty<PuzzleShip>();
            puzzle.sources = new[]
            {
                Source(new Vector3(-4f, 0f, 0f)),
                Source(new Vector3(4f, 0f, 0f))
            };
            return puzzle;
        }

        private PuzzleData TwoShips(Vector3 secondOrigin, Vector3 secondDestination)
        {
            PuzzleData puzzle = Puzzle();
            puzzle.ships = new[]
            {
                Ship(new Vector3(-4f, 0f, 0f), new Vector3(4f, 0f, 0f)),
                Ship(secondOrigin, secondDestination)
            };
            puzzle.sources = Sources(
                new Vector3(-4f, 0f, 0f), new Vector3(4f, 0f, 0f),
                secondOrigin, secondDestination);
            return puzzle;
        }

        private PuzzleData ThreeShipsSharingOrigin()
        {
            PuzzleData puzzle = Puzzle();
            puzzle.ships = new[]
            {
                Ship(new Vector3(-4f, 0f, 0f), new Vector3(4f, 0f, 0f)),
                Ship(new Vector3(-4f, 0f, 0f), new Vector3(0f, 4f, 0f)),
                Ship(new Vector3(-4f, 0f, 0f), new Vector3(-4f, -4f, 0f))
            };
            puzzle.sources = Sources(
                new Vector3(-4f, 0f, 0f), new Vector3(4f, 0f, 0f),
                new Vector3(0f, 4f, 0f), new Vector3(-4f, -4f, 0f));
            return puzzle;
        }

        private PuzzleData Puzzle()
        {
            PuzzleData puzzle = Track(ScriptableObject.CreateInstance<PuzzleData>());
            puzzle.id = "01_01";
            return puzzle;
        }

        private static PuzzleShip Ship(Vector3 origin, Vector3 destination) => new()
        {
            origin = origin,
            destination = destination,
            captureRadius = 0.6f
        };

        private PuzzleSource[] Sources(params Vector3[] points)
        {
            PuzzleSource[] sources = new PuzzleSource[points.Length];
            for (int i = 0; i < points.Length; i++)
                sources[i] = Source(points[i]);

            return sources;
        }

        private static PuzzleSource Source(Vector3 home) => new()
        {
            home = home,
            scale = 1f,
            name = "planet"
        };

        private T Track<T>(T value) where T : Object
        {
            created.Add(value);
            return value;
        }
    }
}
