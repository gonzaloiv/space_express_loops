using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

namespace DigitalLove.Game.Spaceships
{
    public static class SplineContainerExtensions
    {
        public static void SetSplinePositions(this LineRenderer lineRenderer, Vector3[] positions)
        {
            lineRenderer.positionCount = positions.Length;
            for (int i = 0; i < positions.Length; i++)
            {
                lineRenderer.SetPosition(i, positions[i]);
            }
        }

        public static List<Vector3> GetPositions(this SplineContainer splineContainer, int resolution)
        {
            List<Vector3> results = new List<Vector3>(resolution);
            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                results.Add(splineContainer.EvaluatePosition(t));
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