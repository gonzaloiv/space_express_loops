using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;
using UnityEngine.Splines;

namespace DigitalLove.Game.Spaceships
{
    public class RouteContainer : MonoBehaviour
    {
        [SerializeField] private LineRendererWithEdges routeLine;
        [SerializeField] private int resolution = 64;

        private SplineContainer splineContainer;
        private SplineLegSampler sampler;
        private bool isLineVisible;

        private readonly List<RouteLeg> legs = new();
        private int currentLegIndex = -1;

        public IReadOnlyList<RouteLeg> Legs => legs;

        public Vector3 LastLegEndPosition => LastLeg.positions[^1];
        public Vector3 FirstLegEndPosition => legs[0].positions[^1];
        public RouteLeg LastLeg => legs[legs.Count - 1];

        private SplineContainer SplineContainer => splineContainer ??= GetComponent<SplineContainer>();

        public void SetColor(Color color) => routeLine.SetColor(color);

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
            if (isLineVisible)
                RefreshLine();
        }

        public void SetLineRendererActive(bool isVisible)
        {
            isLineVisible = isVisible;
            routeLine.SetVisible(isVisible);
            if (isVisible)
                RefreshLine();
        }

        private bool TryGetCurrentLegPositions(out Vector3[] positions)
        {
            positions = null;
            if (legs.Count == 0 || currentLegIndex < 0 || currentLegIndex >= legs.Count)
                return false;

            positions = legs[currentLegIndex].positions;
            return positions != null && positions.Length > 0;
        }

        private void RefreshLine()
        {
            if (!TryGetCurrentLegPositions(out Vector3[] positions))
            {
                routeLine.Clear();
                return;
            }

            routeLine.SetPositions(positions);
        }
    }
}
