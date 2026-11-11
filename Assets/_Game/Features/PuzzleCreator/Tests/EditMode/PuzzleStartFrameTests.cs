using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleStartFrameTests
    {
        private PuzzleData created;

        [TearDown]
        public void TearDown()
        {
            if (created != null)
                Object.DestroyImmediate(created);
            created = null;
        }

        [Test]
        public void Validate_StartFrameOverShipTwoEndpoint_Fails()
        {
            PuzzleData puzzle = TwoShipPuzzle();

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solution(), Contract(),
                    out List<string> failures));
            Assert.That(failures, Does.Contain("start frame covers an endpoint"));
        }

        [Test]
        public void ContainsEndpoint_ZoneOverShipTwoOrigin_Counts()
        {
            PuzzleZone zone = new()
            {
                centre = new Vector3(0.5f, 0.2f, 0f),
                halfExtents = new Vector3(0.05f, 0.05f, 0.05f),
                scale = 1f,
                falloff = 0.25f,
            };

            Assert.IsTrue(PuzzleZoneCandidates.ContainsEndpoint(zone, TwoShipPuzzle()));
        }

        private PuzzleData TwoShipPuzzle()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            created = puzzle;
            puzzle.origin = new Vector3(-1f, 0.2f, 0f);
            puzzle.destination = new Vector3(1f, 0.2f, 0f);
            puzzle.sources = new[]
            {
                new PuzzleSource { home = Vector3.zero, scale = 0.1f, centreOnly = true },
                new PuzzleSource { home = new Vector3(0f, 0.2f, 0f), scale = 0.1f },
                new PuzzleSource { home = new Vector3(-1f, 0.2f, 0f), scale = 0.1f },
                new PuzzleSource { home = new Vector3(1f, 0.2f, 0f), scale = 0.1f }
            };
            puzzle.ships = new[]
            {
                new PuzzleShip { origin = puzzle.origin, destination = puzzle.destination },
                new PuzzleShip
                {
                    origin = new Vector3(0.5f, 0.2f, 0f),
                    destination = new Vector3(-0.5f, 0.2f, 0f)
                }
            };
            puzzle.zones = 1;
            puzzle.routes = 1;
            puzzle.translatable = true;
            puzzle.zoneCentre = new Vector3(0.5f, 0.2f, 0f);
            puzzle.frames = new[] { new PuzzleFrame { centre = puzzle.zoneCentre, size = 0.1f } };
            return puzzle;
        }

        private static PuzzleJobContract Contract() => new()
        {
            Id = "test", Seed = 1, Attempts = 1, PlanetCount = 3,
            FieldShape = PuzzleFieldShape.Ring,
            HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.Any, MinBlockers = 1,
            Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
            Translatable = true, StartRule = PuzzleStartRule.RunMidpoint,
            NoFail = false, NoGrade = false,
        };

        private static PuzzleZone[] Solution() => new[]
        {
            new PuzzleZone
            {
                centre = new Vector3(0f, 0.2f, 0f),
                halfExtents = new Vector3(0.05f, 0.05f, 0.05f),
                scale = 1f,
                falloff = 0.25f,
            },
        };
    }
}
