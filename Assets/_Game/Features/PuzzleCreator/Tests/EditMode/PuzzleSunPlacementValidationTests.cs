using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleSunPlacementValidationTests
    {
        private readonly List<PuzzleData> created = new();
        private List<string> failures;

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
                Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        [Test]
        public void Validate_CentredSunUnderOffCorridor_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(Vector3.zero));

            Assert.IsFalse(Validate(puzzle, OffCorridor()));
            Assert.That(failures, Does.Contain("sun placement OffCorridor"));
        }

        [Test]
        public void Validate_SunAboveTheBandUnderOffCorridor_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(new Vector3(2f, 0.4f, 0f)));

            Assert.IsFalse(Validate(puzzle, OffCorridor()));
            Assert.That(failures, Does.Contain("sun placement OffCorridor"));
        }

        [Test]
        public void Validate_SunOffTheFieldOriginUnderCentre_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(new Vector3(0.1f, 0.4f, 0f)));

            Assert.IsFalse(Validate(puzzle, new PuzzleJobContract
            {
                Id = "test", PlanetCount = 3, HasSun = true,
                SunPlacement = PuzzleSunPlacement.Centre,
                SunRule = PuzzleSunRule.Any, MinBlockers = 1,
                Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
            }));
            Assert.That(failures, Does.Contain("sun placement Centre"));
        }

        private bool Validate(PuzzleData puzzle, PuzzleJobContract contract)
        {
            bool valid = PuzzleJobValidator.Validate(
                puzzle, PuzzleSolutionKind.Curve, Solved(), contract, out failures);
            return valid;
        }

        private static PuzzleJobContract OffCorridor() => new()
        {
            Id = "test", PlanetCount = 3, HasSun = true,
            SunPlacement = PuzzleSunPlacement.OffCorridor,
            SunRule = PuzzleSunRule.Any, MinBlockers = 1,
            Mechanism = PuzzleMechanism.Curve, Zones = 1, MinRoutes = 1,
        };

        private PuzzleData Puzzle(PuzzleSource sun)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-1f, 0.2f, 0f);
            puzzle.destination = new Vector3(1f, 0.2f, 0f);
            puzzle.sources = new[]
            {
                sun, Planet(new Vector3(0f, 0.2f, 0f)),
                Endpoint(-1f), Endpoint(1f),
            };
            puzzle.zones = 1;
            puzzle.routes = 1;
            created.Add(puzzle);
            return puzzle;
        }

        private static PuzzleZone[] Solved() => new[]
        {
            new PuzzleZone
            {
                centre = new Vector3(0f, 0.2f, 0f),
                halfExtents = new Vector3(0.05f, 0.05f, 0.05f),
                scale = 1f,
                falloff = 0.25f,
            },
        };

        private static PuzzleSource Sun(Vector3 home) =>
            new() { home = home, scale = 0.1f, centreOnly = true };

        private static PuzzleSource Planet(Vector3 home) =>
            new() { home = home, scale = 0.1f };

        private static PuzzleSource Endpoint(float x) =>
            new() { home = new Vector3(x, 0.2f, 0f), scale = 0.1f };
    }
}
