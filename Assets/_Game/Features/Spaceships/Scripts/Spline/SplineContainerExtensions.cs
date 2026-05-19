using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

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

        public static Vector3[] GetPositions(this SplineContainer splineContainer, int resolution, float tStart = 0f, float tEnd = 1f)
        {
            resolution = Mathf.Max(2, resolution);
            Vector3[] results = new Vector3[resolution];
            for (int i = 0; i < resolution; i++)
            {
                float t = Mathf.Lerp(tStart, tEnd, i / (float)(resolution - 1));
                results[i] = splineContainer.EvaluatePosition(t);
            }
            return results;
        }

        public static void SetKnotPosition(this SplineContainer splineContainer, int index, Vector3 position)
        {
            BezierKnot knot = splineContainer.Spline[index];
            knot.Position = position;
            splineContainer.Spline.SetKnot(index, knot);
        }

        public static void SetKnotRotation(this SplineContainer splineContainer, int index, Vector3 rotation)
        {
            BezierKnot knot = splineContainer.Spline[index];
            knot.Rotation = quaternion.Euler(rotation);
            splineContainer.Spline.SetKnot(index, knot);
        }
    }
}