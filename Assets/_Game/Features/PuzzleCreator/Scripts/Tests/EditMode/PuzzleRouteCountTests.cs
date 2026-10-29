using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleRouteCountTests
    {
        private SpaceShapingSettings settings;
        private PuzzleSimSettings sim;
        private PuzzleData puzzle;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            sim = PuzzleSimSettings.From(settings);
            puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = new Vector3(-0.35f, 0f, 0f);
            puzzle.destination = new Vector3(0.35f, 0f, 0f);
            puzzle.launchSpeed = 1f;
            puzzle.cubeSizeMin = settings.cubeSizeMin;
            puzzle.cubeSizeMax = settings.cubeSizeMax;
            puzzle.cubeSize = 0.21f;
            puzzle.tmin = 0f;
            puzzle.tmax = 100f;
            puzzle.entropyBudget = 100f;
            puzzle.sources = new[]
            {
                new PuzzleSource { home = new Vector3(0f, 0.2f, 0f), scale = 0.1f },
                new PuzzleSource { home = new Vector3(0f, -0.2f, 0f), scale = 0.1f }
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (puzzle != null)
                Object.DestroyImmediate(puzzle);

            if (settings != null)
                Object.DestroyImmediate(settings);
        }

        [Test]
        public void Count_FindsASecondRouteOnTheOtherSource()
        {
            PuzzleZone[] solution =
            {
                PuzzleZoneCandidates.Zone(
                    puzzle.sources[0].home, 0.21f, 1.4f, settings.zoneFalloff)
            };

            Assert.GreaterOrEqual(PuzzleRouteCount.Count(puzzle, sim, solution), 2);
        }

        [Test]
        public void Count_WithoutASolution_IsZero()
        {
            Assert.AreEqual(0, PuzzleRouteCount.Count(puzzle, sim, null));
        }

        [Test]
        public void Count_OnAnEmptyField_IsOne()
        {
            puzzle.sources = System.Array.Empty<PuzzleSource>();
            PuzzleZone[] solution =
            {
                PuzzleZoneCandidates.Zone(Vector3.zero, 0.21f, 1.4f, settings.zoneFalloff)
            };

            Assert.AreEqual(1, PuzzleRouteCount.Count(puzzle, sim, solution));
        }
    }
}
