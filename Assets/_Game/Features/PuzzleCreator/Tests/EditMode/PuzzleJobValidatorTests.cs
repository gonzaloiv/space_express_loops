using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleJobValidatorTests
    {
        private readonly List<PuzzleData> created = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
                Object.DestroyImmediate(created[i]);
            created.Clear();
        }

        [Test]
        public void Validate_ValidSunCurve_Passes()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));

            Assert.IsTrue(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solved(), Contract(PuzzleMechanism.Curve),
                    out List<string> failures),
                string.Join("\n", failures));
        }

        [Test]
        public void Validate_SunCentredGrown_Passes()
        {
            PuzzleSource sun = Sun();
            sun.scale = 0.07f;
            PuzzleData puzzle = Puzzle(sun, Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.SunRule = PuzzleSunRule.CentredGrown;

            Assert.IsTrue(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Zone(Vector3.zero, 1.4f), contract,
                    out List<string> failures),
                string.Join("\n", failures));
        }

        [Test]
        public void Validate_SunLargerThanCube_Fails()
        {
            PuzzleSource sun = Sun();
            sun.scale = 0.2f;
            PuzzleData puzzle = Puzzle(sun, Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.SunRule = PuzzleSunRule.CentredGrown;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Zone(Vector3.zero, 1.4f), contract,
                    out List<string> failures));
            Assert.That(failures, Does.Contain("sun rule CentredGrown failed"));
        }

        [Test]
        public void Validate_SunNotGrown_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.SunRule = PuzzleSunRule.CentredGrown;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Zone(Vector3.zero, 1f), contract,
                    out List<string> failures));
            Assert.That(failures, Does.Contain("sun rule CentredGrown failed"));
        }

        [Test]
        public void Validate_SunOutOfZone_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.SunRule = PuzzleSunRule.OutOfZone;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Zone(Vector3.zero, 1.4f), contract,
                    out List<string> failures));
            Assert.That(failures, Does.Contain("sun rule OutOfZone failed"));
        }

        [Test]
        public void Validate_SunRuleMismatch_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.SunRule = PuzzleSunRule.CentredGrown;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solved(), contract,
                    out List<string> failures));
            Assert.That(failures, Does.Contain("sun rule CentredGrown failed"));
        }

        [Test]
        public void Validate_WrongRole_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Clear, Solved(), Contract(PuzzleMechanism.Curve),
                    out List<string> failures));
            Assert.That(failures, Does.Contain("role Clear, expected Curve"));
        }

        [Test]
        public void Validate_TooFewBlockers_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.MinBlockers = 2;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solved(), contract, out List<string> failures));
            Assert.That(failures, Does.Contain("blockers 1, expected at least 2"));
        }

        [Test]
        public void Validate_WrongPlanetCount_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.PlanetCount = 4;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solved(), contract, out List<string> failures));
            Assert.That(failures, Does.Contain("planets 3, expected 4"));
        }

        [Test]
        public void Validate_SunOnChordMismatch_Fails()
        {
            PuzzleData puzzle = PuzzleOnChord();
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.MinBlockers = 0;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solved(), contract, out List<string> failures));
            Assert.That(failures, Does.Contain("sun on chord True, expected False"));
        }

        [Test]
        public void Validate_TooFewRoutes_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            puzzle.routes = 1;
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.MinRoutes = 2;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solved(), contract, out List<string> failures));
            Assert.That(failures, Does.Contain("routes 1, expected at least 2"));
        }

        [Test]
        public void Validate_WrongTutorialFlags_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            puzzle.noFail = true;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solved(), Contract(PuzzleMechanism.Curve),
                    out List<string> failures));
            Assert.That(failures, Does.Contain("noFail True, expected False"));
        }

        [Test]
        public void Validate_NullPuzzle_Fails()
        {
            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    null, PuzzleSolutionKind.Curve, Solved(), Contract(PuzzleMechanism.Curve),
                    out List<string> failures));
            Assert.That(failures, Does.Contain("no puzzle"));
        }

        [Test]
        public void Validate_MissingSolution_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, null, Contract(PuzzleMechanism.Curve),
                    out List<string> failures));
            Assert.That(failures, Does.Contain("no solution zones"));
        }

        [Test]
        public void Validate_MovableStartFrame_Fails()
        {
            PuzzleData puzzle = Puzzle(Sun(), Planet(Blocker()), Endpoint(-1f), Endpoint(1f));
            puzzle.translatable = true;
            puzzle.zoneCentre = new Vector3(0.1f, 0f, 0f);
            puzzle.frames = new[] { new PuzzleFrame { centre = puzzle.zoneCentre, size = 0.1f } };
            PuzzleJobContract contract = Contract(PuzzleMechanism.Curve);
            contract.Translatable = true;
            contract.StartRule = PuzzleStartRule.FieldCentre;

            Assert.IsFalse(
                PuzzleJobValidator.Validate(
                    puzzle, PuzzleSolutionKind.Curve, Solved(), contract, out List<string> failures));
            Assert.That(failures, Has.Some.Contains("start frame"));
        }

        private PuzzleData Puzzle(params PuzzleSource[] sources)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-1f, 0.2f, 0f);
            puzzle.destination = new Vector3(1f, 0.2f, 0f);
            puzzle.sources = sources;
            puzzle.zones = 1;
            puzzle.routes = 1;
            puzzle.translatable = false;
            puzzle.noFail = false;
            puzzle.noGrade = false;
            created.Add(puzzle);
            return puzzle;
        }

        private PuzzleData PuzzleOnChord()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-1f, 0f, 0f);
            puzzle.destination = new Vector3(1f, 0f, 0f);
            puzzle.sources = new[] { Sun(), Planet(new Vector3(0f, 0.2f, 0f)), Endpoint(-1f), Endpoint(1f) };
            puzzle.zones = 1;
            puzzle.routes = 1;
            puzzle.translatable = false;
            puzzle.noFail = false;
            puzzle.noGrade = false;
            created.Add(puzzle);
            return puzzle;
        }

        private static PuzzleJobContract Contract(PuzzleMechanism mechanism) => new()
        {
            Id = "test", Seed = 1, Attempts = 1, PlanetCount = 3,
            FieldShape = PuzzleFieldShape.Ring,
            HasSun = true, SunOnChord = false, SunRule = PuzzleSunRule.Any, MinBlockers = 1,
            Mechanism = mechanism, Zones = 1, MinRoutes = 1,
            Translatable = false, StartRule = PuzzleStartRule.RunMidpoint,
            NoFail = false, NoGrade = false,
        };

        private static PuzzleZone[] Solved() => Zone(Blocker());

        private static PuzzleZone[] Zone(Vector3 centre, float scale = 1f) => new[]
        {
            new PuzzleZone
            {
                centre = centre,
                halfExtents = new Vector3(0.05f, 0.05f, 0.05f),
                scale = scale,
                falloff = 0.25f,
            },
        };

        private static PuzzleSource Sun() =>
            new() { home = Vector3.zero, scale = 0.1f, centreOnly = true };

        private static PuzzleSource Planet(Vector3 home) =>
            new() { home = home, scale = 0.1f };

        private static Vector3 Blocker() => new(0f, 0.2f, 0f);

        private static PuzzleSource Endpoint(float x) =>
            new() { home = new Vector3(x, 0.2f, 0f), scale = 0.1f };
    }
}
