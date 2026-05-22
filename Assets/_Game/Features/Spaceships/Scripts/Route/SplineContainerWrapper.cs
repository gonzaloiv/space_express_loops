using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class SplineContainerWrapper : MonoBehaviour
    {
        [SerializeField] private LineRendererWithEdges goLine;
        [SerializeField] private LineRendererWithEdges returnLine;
        [SerializeField] private int resolution = 64;

        private SplineContainer splineContainer;
        private SplineLegSampler sampler;
        private bool isLineVisible;

        private readonly List<RouteLeg> legs = new();
        private int currentLegIndex = -1;

        public IReadOnlyList<RouteLeg> Legs => legs;
        public bool HasLegs => legs.Count > 0;

        public Vector3 LastLegEndPosition => LastLeg.positions[^1];
        public Vector3 FirstLegEndPosition => legs[0].positions[^1];
        public RouteLeg LastLeg => legs[legs.Count - 1];

        private SplineContainer SplineContainer => splineContainer ??= GetComponent<SplineContainer>();

        public void SetColor(Color color) => goLine.SetColor(color);

        public Vector3 GetPanelAnchorPosition()
        {
            if (!TryGetCurrentLegPositions(out Vector3[] positions))
                return transform.position;

            return positions[positions.Length / 2];
        }

        public void Build(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations)
        {
            legs.Clear();
            currentLegIndex = -1;

            if (hub == null || destinations == null || destinations.Count == 0)
            {
                ClearLines();
                return;
            }

            sampler ??= new SplineLegSampler(SplineContainer, resolution);
            sampler.Build(hub, destinations, legs);

            ShowLeg(0);
            RefreshReturnLine();
        }

        public void ShowLeg(int legIndex)
        {
            currentLegIndex = legIndex;
            if (isLineVisible)
            {
                RefreshGoLine();
                RefreshReturnLine();
            }
        }

        public void SetLineRendererActive(bool isVisible)
        {
            isLineVisible = isVisible;
            goLine.SetVisible(isVisible);
            returnLine.SetVisible(isVisible);

            if (isVisible)
            {
                RefreshGoLine();
                RefreshReturnLine();
            }
            else
                ClearLines();
        }

        private bool TryGetCurrentLegPositions(out Vector3[] positions)
        {
            positions = null;
            if (legs.Count == 0 || currentLegIndex < 0 || currentLegIndex >= legs.Count)
                return false;

            positions = legs[currentLegIndex].positions;
            return positions != null && positions.Length > 0;
        }

        private bool TryGetReturnLegPositions(out Vector3[] positions)
        {
            positions = null;
            if (legs.Count < 2 || currentLegIndex < 0)
                return false;

            int returnLegIndex = (currentLegIndex + 1) % legs.Count;
            positions = legs[returnLegIndex].positions;
            return positions != null && positions.Length > 0;
        }

        private void RefreshGoLine()
        {
            if (!TryGetCurrentLegPositions(out Vector3[] positions))
            {
                goLine.Clear();
                return;
            }

            goLine.SetPositions(positions);
        }

        private void RefreshReturnLine()
        {
            if (!TryGetReturnLegPositions(out Vector3[] positions))
            {
                returnLine.Clear();
                return;
            }

            returnLine.SetPositions(positions);
        }

        private void ClearLines()
        {
            goLine.Clear();
            returnLine.Clear();
        }
    }
}
