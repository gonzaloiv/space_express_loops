using System.Collections.Generic;
using DigitalLove.Game.Planets;
using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class RouteContainer : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int resolution = 64;

        private SplineContainer splineContainer;
        private SplineLegSampler sampler;

        private readonly List<RouteLeg> legs = new();
        private int currentLegIndex = -1;

        public IReadOnlyList<RouteLeg> Legs => legs;

        private SplineContainer SplineContainer => splineContainer ??= GetComponent<SplineContainer>();

        public void SetColor(Color color) => lineRenderer.material.color = color;

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
                return;

            sampler ??= new SplineLegSampler(SplineContainer, resolution);
            sampler.Build(hub, destinations, legs);

            ShowLeg(0);
        }

        public void ShowLeg(int legIndex)
        {
            currentLegIndex = legIndex;
            if (lineRenderer.enabled)
                RefreshLineRenderer();
        }

        public void SetLineRendererActive(bool isVisible)
        {
            lineRenderer.enabled = isVisible;
            if (!isVisible)
                lineRenderer.positionCount = 0;
            else
                RefreshLineRenderer();
        }

        private bool TryGetCurrentLegPositions(out Vector3[] positions)
        {
            positions = null;
            if (legs.Count == 0 || currentLegIndex < 0 || currentLegIndex >= legs.Count)
                return false;

            positions = legs[currentLegIndex].positions;
            return positions != null && positions.Length > 0;
        }

        private void RefreshLineRenderer()
        {
            if (!TryGetCurrentLegPositions(out Vector3[] positions))
            {
                lineRenderer.positionCount = 0;
                return;
            }

            lineRenderer.SetSplinePositions(positions);
        }
    }
}
