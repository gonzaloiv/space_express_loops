using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleRunGeometryTests
    {
        [Test]
        public void MaxAngle_StraightPath_IsZero()
        {
            TrajectoryResult path = Path(
                new Vector3(0f, 0f, 0f),
                new Vector3(0.5f, 0f, 0f),
                new Vector3(1f, 0f, 0f));

            float angle = PuzzleRunGeometry.MaxAngle(path, Vector3.zero, Vector3.right);

            Assert.AreEqual(0f, angle, 0.001f);
        }

        [Test]
        public void MaxChordDeviation_StraightPath_IsZero()
        {
            TrajectoryResult path = Path(
                new Vector3(0f, 0f, 0f),
                new Vector3(0.5f, 0f, 0f),
                new Vector3(1f, 0f, 0f));

            float deviation = PuzzleRunGeometry.MaxChordDeviation(
                path, Vector3.zero, Vector3.right);

            Assert.AreEqual(0f, deviation, 0.001f);
        }

        [Test]
        public void MaxChordDeviation_BowedPath_IsPositive()
        {
            TrajectoryResult path = Path(
                new Vector3(0f, 0f, 0f),
                new Vector3(0.5f, 0.1f, 0f),
                new Vector3(1f, 0f, 0f));

            float deviation = PuzzleRunGeometry.MaxChordDeviation(
                path, Vector3.zero, Vector3.right);

            Assert.AreEqual(0.1f, deviation, 0.001f);
        }

        [Test]
        public void MaxAngle_BentPath_IsPositive()
        {
            TrajectoryResult path = Path(
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(1f, 1f, 0f));

            float angle = PuzzleRunGeometry.MaxAngle(path, Vector3.zero, Vector3.right);

            Assert.Greater(angle, 45f);
        }

        private static TrajectoryResult Path(params Vector3[] points) =>
            new(TrajectoryOutcome.Miss, new List<Vector3>(points));
    }
}
