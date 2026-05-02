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

        public static Vector3[] GetPositions(this SplineContainer splineContainer, int resolution)
        {
            Vector3[] results = new Vector3[resolution];
            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                Vector3 localPos = splineContainer.EvaluatePosition(t);
                results[i] = localPos;
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