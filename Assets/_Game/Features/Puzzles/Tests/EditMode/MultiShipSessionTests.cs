using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class MultiShipSessionTests
    {
        [Test]
        public void AllShipsArrive_IsSolved()
        {
            SpaceShapingSession session = new(TwoShips(new Vector3(4f, 2f, 0f)));

            TrajectoryResult[] results = session.SimulateAll();

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual(TrajectoryOutcome.Arrive, results[0].Outcome);
            Assert.AreEqual(TrajectoryOutcome.Arrive, results[1].Outcome);
            Assert.IsTrue(session.IsSolved(results));
        }

        [Test]
        public void OneShipMisses_IsNotSolved()
        {
            SpaceShapingSession session = new(TwoShips(new Vector3(400f, 0f, 0f)));

            TrajectoryResult[] results = session.SimulateAll();

            Assert.AreEqual(TrajectoryOutcome.Arrive, results[0].Outcome);
            Assert.AreEqual(TrajectoryOutcome.Miss, results[1].Outcome);
            Assert.IsFalse(session.IsSolved(results));
        }

        [Test]
        public void EmptyShips_FallsBackToSingleRun()
        {
            SpaceShapingSession session = new(TestSetups.Session(1200));

            TrajectoryResult[] results = session.SimulateAll();

            Assert.AreEqual(1, results.Length);
            Assert.IsTrue(session.IsSolved(results));
        }

        [Test]
        public void Budget_IsMaxOverRuns()
        {
            PuzzleData puzzle = Puzzle();
            PuzzleSimSettings settings = new()
            {
                NominalSpeed = 1f,
                StepSecs = 1f / 120f,
                CaptureRadius = 0.6f
            };

            SpaceShapingSetup setup = SpaceShapingSetup.Build(puzzle, settings);
            Object.DestroyImmediate(puzzle);

            Assert.AreEqual(9600, setup.MaxSteps);
        }

        [Test]
        public void ShipEndingAt_NamesTheShipOnThatSource()
        {
            PuzzleData puzzle = EndpointPuzzle();
            SpaceShapingSession session = new(SpaceShapingSetup.Build(puzzle, new PuzzleSimSettings
            {
                NominalSpeed = 1f,
                StepSecs = 1f / 120f,
                CaptureRadius = 0.6f
            }));
            Object.DestroyImmediate(puzzle);

            Assert.AreEqual(0, session.ShipEndingAt(1));
            Assert.AreEqual(1, session.ShipEndingAt(2));
            Assert.AreEqual(1, session.ShipEndingAt(3));
            Assert.AreEqual(-1, session.ShipEndingAt(4));
        }

        private static PuzzleData EndpointPuzzle()
        {
            PuzzleData puzzle = Puzzle();
            puzzle.ships[1].origin = new Vector3(-4f, 2f, 0f);
            puzzle.ships[1].destination = new Vector3(4f, 2f, 0f);
            puzzle.sources = new[]
            {
                new PuzzleSource { home = new Vector3(-4f, 0f, 0f), scale = 0.5f },
                new PuzzleSource { home = new Vector3(4f, 0f, 0f), scale = 0.5f },
                new PuzzleSource { home = new Vector3(-4f, 2f, 0f), scale = 0.5f },
                new PuzzleSource { home = new Vector3(4f, 2f, 0f), scale = 0.5f },
                new PuzzleSource { home = new Vector3(0f, 5f, 0f), scale = 0.5f }
            };
            return puzzle;
        }

        private static SpaceShapingSetup TwoShips(Vector3 secondDestination)
        {
            SpaceShapingSetup setup = TestSetups.Session(1200);
            setup.Ships = new[]
            {
                new ShipSpec
                {
                    Origin = new Vector3(-4f, 0f, 0f),
                    Destination = new Vector3(4f, 0f, 0f)
                },
                new ShipSpec
                {
                    Origin = new Vector3(-4f, 2f, 0f),
                    Destination = secondDestination
                }
            };
            return setup;
        }

        private static PuzzleData Puzzle()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-4f, 0f, 0f);
            puzzle.destination = new Vector3(4f, 0f, 0f);
            puzzle.sources = System.Array.Empty<PuzzleSource>();
            puzzle.ships = new[]
            {
                new PuzzleShip
                {
                    origin = new Vector3(-4f, 0f, 0f),
                    destination = new Vector3(4f, 0f, 0f),
                    captureRadius = 0.6f
                },
                new PuzzleShip
                {
                    origin = new Vector3(-40f, 0f, 0f),
                    destination = new Vector3(40f, 0f, 0f),
                    captureRadius = 0.6f
                }
            };
            return puzzle;
        }
    }
}
