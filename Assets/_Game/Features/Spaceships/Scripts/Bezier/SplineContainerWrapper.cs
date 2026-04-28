using System.Linq;
using DigitalLove.Game.Planets;
using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class SplineContainerWrapper : MonoBehaviour
    {
        [SerializeField] private int resolution = 50;

        private SplineContainer splineContainer;
        private Vector3[] positions;

        public Vector3[] Positions => positions;
        public Vector3[] GoPositions => Positions.Skip(0).Take(resolution / 2).ToArray();
        public Vector3[] ReturnPositions => Positions.Skip(resolution / 2).Take(resolution / 2).ToArray();
        public Vector3 OriginPosition => positions[0];

        private SplineContainer SplineContainer => splineContainer ??= GetComponent<SplineContainer>();

        public void CreateLoop(BasePlanetBehaviour basePlanet, PlanetBehaviour destinationPlanet)
        {
            Vector3 direction = (destinationPlanet.transform.position - basePlanet.transform.position).normalized;
            float baseRadiusOffset = basePlanet.RadiusOffset;

            SplineContainer.transform.position = basePlanet.transform.position - direction * baseRadiusOffset * 1.25f;
            SplineContainer.transform.forward = direction;

            Vector3 destinationPosition = destinationPlanet.transform.position + direction * destinationPlanet.RadiusOffset * 1.25f;
            SplineContainer.SetKnotPosition(3, SplineContainer.transform.InverseTransformPoint(destinationPosition));

            Vector3 oneThirdPosition = SplineContainer.transform.position + direction * baseRadiusOffset * 1.25f;
            Vector3 originOffset = SplineContainer.transform.right.normalized * baseRadiusOffset;

            Vector3 oneThirdRight = SplineContainer.transform.InverseTransformPoint(oneThirdPosition + originOffset);
            SplineContainer.SetKnotPosition(1, oneThirdRight);
            Vector3 oneThirdLeft = SplineContainer.transform.InverseTransformPoint(oneThirdPosition - originOffset);
            SplineContainer.SetKnotPosition(5, oneThirdLeft);

            float destinationRadiusOffset = destinationPlanet.RadiusOffset;
            Vector3 twoThirdsPosition = destinationPosition - direction * destinationRadiusOffset * 1.25f;
            Vector3 destinationOffset = SplineContainer.transform.right.normalized * destinationRadiusOffset;

            // ? Inverted for loop 
            Vector3 twoThirdsRight = SplineContainer.transform.InverseTransformPoint(twoThirdsPosition + destinationOffset);
            SplineContainer.SetKnotPosition(4, twoThirdsRight);

            Vector3 twoThirdsLeft = SplineContainer.transform.InverseTransformPoint(twoThirdsPosition - destinationOffset);
            SplineContainer.SetKnotPosition(2, twoThirdsLeft);

            positions = SplineContainer.GetPositions(resolution);
        }
    }
}