using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

namespace DigitalLove.Game.Spaceships
{
    public static class SplineContainerExtensions
    {
        public static List<Vector3> GetPositions(
            this SplineContainer splineContainer,
            int resolution,
            Transform contentRoot = null,
            float tMax = 1f)
        {
            List<Vector3> results = new List<Vector3>(resolution);
            for (int i = 0; i < resolution; i++)
            {
                float t = tMax * i / (resolution - 1);
                Vector3 position = splineContainer.EvaluatePosition(t);
                if (contentRoot != null)
                    position = contentRoot.InverseTransformPoint(position);
                results.Add(position);
            }
            return results;
        }

        public static void SetKnotPosition(this SplineContainer splineContainer, int index, Vector3 position)
        {
            BezierKnot knot = splineContainer.Spline[index];
            knot.Position = position;
            splineContainer.Spline.SetKnot(index, knot);
        }
    }
}