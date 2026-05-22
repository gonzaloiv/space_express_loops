using UnityEngine;
using DigitalLove.Game.Nodes;
using DigitalLove.Global;

namespace DigitalLove.Game.Spaceships
{
    public class RaycastHelper : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private LineRendererWithEdges lineRendererWithEdges;
        [SerializeField] private Transform lineOrigin;
        [SerializeField] private FloatValue rayDistance;

        [SerializeField] private float rayForwardOffset = 0.1f;

        private bool isActive;
        private PlanetBehaviour candidatePlanet;

        public PlanetBehaviour CandidatePlanet => candidatePlanet;
        public float RayDistance => rayDistance.value * 1.1f;

        public void SetActive(bool active)
        {
            isActive = active;
            if (!active)
                candidatePlanet = null;
            lineRendererWithEdges.SetVisible(active);
        }

        public void SetColor(Color color) => lineRendererWithEdges.SetColor(color);

        private void Update()
        {
            if (!isActive)
                return;

            CheckHits();
            UpdateLine();
        }

        private void CheckHits()
        {
            candidatePlanet = null;
            float closestSqrDistance = float.MaxValue;
            Vector3 origin = transform.position;

            foreach (RaycastHit hit in Physics.RaycastAll(origin, transform.forward, RayDistance, layerMask))
            {
                if (hit.rigidbody == null)
                    continue;

                PlanetBehaviour planet = hit.rigidbody.GetComponent<PlanetBehaviour>();
                if (planet == null)
                    continue;

                float sqrDistance = (hit.point - origin).sqrMagnitude;
                if (sqrDistance >= closestSqrDistance)
                    continue;

                closestSqrDistance = sqrDistance;
                candidatePlanet = planet;
            }
        }

        private void UpdateLine()
        {
            if (lineOrigin == null)
                return;

            Vector3 forward = transform.forward;
            Vector3 start = lineOrigin.position + forward * rayForwardOffset;
            Vector3 end = start + forward * RayDistance;

            if (candidatePlanet != null)
            {
                float distance = Vector3.Distance(candidatePlanet.transform.position, start);
                Vector3 offset = forward * (candidatePlanet.NodeBody.Radius - rayForwardOffset);
                end = start + forward * distance - offset;
            }

            lineRendererWithEdges.SetTwoPositions(start, end);
        }
    }
}
