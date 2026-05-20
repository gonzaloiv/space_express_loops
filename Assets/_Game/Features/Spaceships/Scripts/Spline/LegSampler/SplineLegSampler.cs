using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class SplineLegSampler
    {
        private readonly SplineContainer splineContainer;
        private readonly int samplesPerSegment;

        public SplineLegSampler(SplineContainer splineContainer, int samplesPerSegment)
        {
            this.splineContainer = splineContainer;
            this.samplesPerSegment = samplesPerSegment;
        }

        public void SampleHalves(
            Vector3 originPosition,
            float originRadius,
            Vector3 originForward,
            Vector3 destinationPosition,
            float destinationRadius,
            float outboundEndT,
            float inboundStartT,
            out Vector3[] outboundHalf,
            out Vector3[] inboundHalf)
        {
            ConfigureSplineForLeg(originPosition, originRadius, originForward, destinationPosition, destinationRadius);
            outboundHalf = splineContainer.GetPositions(samplesPerSegment, 0f, outboundEndT);
            inboundHalf = splineContainer.GetPositions(samplesPerSegment, inboundStartT, 1f);
        }

        public Vector3[] SampleRange(
            Vector3 originPosition,
            float originRadius,
            Vector3 originForward,
            Vector3 destinationPosition,
            float destinationRadius,
            float tStart,
            float tEnd)
        {
            ConfigureSplineForLeg(originPosition, originRadius, originForward, destinationPosition, destinationRadius);
            return splineContainer.GetPositions(samplesPerSegment, tStart, tEnd);
        }

        private void ConfigureSplineForLeg(
            Vector3 originPosition,
            float originRadius,
            Vector3 originForward,
            Vector3 destinationPosition,
            float destinationRadius)
        {
            Vector3 direction = (destinationPosition - originPosition).normalized;
            if (direction.sqrMagnitude < 0.0001f)
                direction = originForward;

            splineContainer.transform.position = originPosition - direction * originRadius * 1.25f;
            splineContainer.transform.forward = direction;

            Vector3 destinationApproach = destinationPosition + direction * destinationRadius * 1.25f;
            splineContainer.SetKnotPosition(3, splineContainer.transform.InverseTransformPoint(destinationApproach));

            Vector3 oneThirdPosition = splineContainer.transform.position + direction * originRadius * 1.25f;
            Vector3 originOffset = splineContainer.transform.right.normalized * originRadius;

            splineContainer.SetKnotPosition(1, splineContainer.transform.InverseTransformPoint(oneThirdPosition + originOffset));
            splineContainer.SetKnotPosition(5, splineContainer.transform.InverseTransformPoint(oneThirdPosition - originOffset));

            Vector3 twoThirdsPosition = destinationApproach - direction * destinationRadius * 1.25f;
            Vector3 destinationOffset = splineContainer.transform.right.normalized * destinationRadius;

            splineContainer.SetKnotPosition(4, splineContainer.transform.InverseTransformPoint(twoThirdsPosition + destinationOffset));
            splineContainer.SetKnotPosition(2, splineContainer.transform.InverseTransformPoint(twoThirdsPosition - destinationOffset));
        }
    }
}
