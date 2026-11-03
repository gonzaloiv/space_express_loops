using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class TrajectoryCapTests
    {
        private static TrajectoryResult Path(params Vector3[] points) =>
            new(TrajectoryOutcome.Crash, new List<Vector3>(points));

        [Test]
        public void Clip_ShortCrash_StartsAtTheLaunchCap()
        {
            TrajectoryResult result = Path(Vector3.zero, new Vector3(2f, 0f, 0f));

            TrajectoryResult clipped = TrajectoryCaps.Clip(
                result, Vector3.zero, 0.6f, new Vector3(10f, 0f, 0f), 0.5f);

            Assert.AreEqual(2, clipped.Points.Count);
            Assert.AreEqual(0.6f, clipped.Points[0].x, 0.0001f);
            Assert.Greater(Vector3.Distance(clipped.Points[0], Vector3.zero), 0.5f);
        }

        [Test]
        public void Clip_EndsAtTheArrivalCap()
        {
            TrajectoryResult result = Path(
                Vector3.zero, new Vector3(2f, 0f, 0f), new Vector3(4f, 0f, 0f));

            TrajectoryResult clipped = TrajectoryCaps.Clip(
                result, Vector3.zero, 0.6f, new Vector3(4f, 0f, 0f), 0.5f);

            Assert.AreEqual(3, clipped.Points.Count);
            Assert.AreEqual(3.5f, clipped.Points[2].x, 0.0001f);
            Assert.AreEqual(0.5f, Vector3.Distance(clipped.Points[2], new Vector3(4f, 0f, 0f)), 0.0001f);
        }

        [Test]
        public void Clip_PathInsideBothCaps_HasNoPoints()
        {
            TrajectoryResult result = Path(Vector3.zero, new Vector3(0.2f, 0f, 0f));

            TrajectoryResult clipped = TrajectoryCaps.Clip(
                result, Vector3.zero, 0.6f, new Vector3(10f, 0f, 0f), 0.5f);

            Assert.IsFalse(clipped.HasPoints);
        }
    }
}
