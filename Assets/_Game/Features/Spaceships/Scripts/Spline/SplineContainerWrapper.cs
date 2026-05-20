using System.Collections.Generic;
using DigitalLove.Game.Planets;
using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class SplineContainerWrapper : MonoBehaviour
    {
        [SerializeField] private int samplesPerHalf = 32;
        [SerializeField] private LineRenderer lineRenderer;

        private SplineContainer splineContainer;
        private SplineLegSampler sampler;
        private SinglePlanetRouteBuilder singlePlanetRouteBuilder;
        private MultiPlanetRouteBuilder multiPlanetRouteBuilder;

        private readonly List<RouteLegPath> legs = new();

        public IReadOnlyList<RouteLegPath> Legs => legs;

        private SplineContainer SplineContainer => splineContainer ??= GetComponent<SplineContainer>();

        private void EnsureBuilders()
        {
            if (sampler != null)
                return;

            sampler = new SplineLegSampler(SplineContainer, samplesPerHalf);
            singlePlanetRouteBuilder = new SinglePlanetRouteBuilder(sampler);
            multiPlanetRouteBuilder = new MultiPlanetRouteBuilder(sampler);
        }

        public void SetColor(Color color) => lineRenderer.material.color = color;

        public Vector3 GetPanelAnchorPosition()
        {
            if (legs.Count == 0 || legs[0].Positions == null || legs[0].Positions.Length == 0)
                return transform.position;

            Vector3[] firstLeg = legs[0].Positions;
            return firstLeg[firstLeg.Length / 2];
        }

        public void BuildLoop(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations)
        {
            legs.Clear();

            if (hub == null || destinations == null || destinations.Count == 0)
                return;

            EnsureBuilders();
            IRouteLoopBuilder builder = destinations.Count == 1
                ? singlePlanetRouteBuilder
                : multiPlanetRouteBuilder;
            builder.Build(hub, destinations, legs);

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

        private void RefreshLineRenderer()
        {
            Vector3[] merged = legs.Merge();
            if (merged.Length == 0)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            lineRenderer.SetSplinePositions(merged);
        }
    }
}
