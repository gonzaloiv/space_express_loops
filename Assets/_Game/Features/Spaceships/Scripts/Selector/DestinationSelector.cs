using DigitalLove.Game.Planets;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class DestinationSelector : MonoBehaviour
    {
        [SerializeField] private RaycastHelper raycastHelper;
        [SerializeField] private float secsToSelect = 0.5f;
        [SerializeField] private float initialSecsToSelect = 0.2f;
        [SerializeField] private GameObject destinationZone;
        [SerializeField] private Renderer zoneRenderer;
        [SerializeField] private ColorValue insideColor;
        [SerializeField] private float destinationZoneRadius = 0.5f;

        private float countdown;
        private bool isLookingForDestination;

        [Header("DEBUG")]
        private PlanetBaseBehaviour basePlanet;
        private PlanetBehaviour destinationPlanet;

        public PlanetBaseBehaviour BasePlanet => basePlanet;
        public PlanetBehaviour Destination => destinationPlanet;
        public bool HasDestinationBeenSelected => destinationPlanet != null && destinationPlanet.IsDestination;

        public void StartLookingForDestination(bool isLookingForDestination)
        {
            this.isLookingForDestination = isLookingForDestination;
            raycastHelper.SetActive(isLookingForDestination);
        }

        public void SetBase(PlanetBaseBehaviour basePlanet)
        {
            this.basePlanet = basePlanet;
            raycastHelper.SetBase(basePlanet);
        }

        private void Update()
        {
            if (isLookingForDestination)
            {
                CheckHits();
                UpdateDestinationCandidate();
            }
            UpdateDestinationZone();
        }

        private void CheckHits()
        {
            PlanetBehaviour candidatePlanet = raycastHelper.CandidatePlanet;
            if (candidatePlanet == null)
            {
                if (destinationPlanet != null)
                    DeselectCurrent();
            }
            else
            {
                bool isValidNewDestination = candidatePlanet != destinationPlanet && candidatePlanet.gameObject != basePlanet.gameObject && candidatePlanet.CanBeDestination;
                if (isValidNewDestination)
                    SelectNewDestination(candidatePlanet);
            }
        }

        private void DeselectCurrent()
        {
            destinationPlanet.SetIsDestination(false);
            destinationPlanet = null;
        }

        private void SelectNewDestination(PlanetBehaviour newDestination)
        {
            if (destinationPlanet != null)
                destinationPlanet.SetIsDestination(false);
            destinationPlanet = newDestination;
            countdown = secsToSelect + initialSecsToSelect;
        }

        private void UpdateDestinationCandidate()
        {
            if (destinationPlanet != null)
            {
                countdown -= Time.deltaTime;
                if (countdown <= secsToSelect)
                {
                    if (countdown <= 0 && !HasDestinationBeenSelected)
                    {
                        OnDestinationSelected();
                    }
                }
            }
        }

        private void OnDestinationSelected()
        {
            destinationPlanet.SetIsDestination(true);
            raycastHelper.SetVisible(false);
        }

        private void UpdateDestinationZone()
        {
            if (isLookingForDestination && destinationPlanet != null && countdown <= secsToSelect)
            {
                destinationZone.transform.position = destinationPlanet.Position;

                float t = Mathf.Clamp01(1f - (countdown / secsToSelect));
                float fromScale = 1f;
                float toScale = 2f;
                float currentScale = Mathf.Lerp(fromScale, toScale, t);
                destinationZone.transform.localScale = Vector3.one * currentScale;

                Color outsideColor = Color.white;
                float zoneRadius = destinationZone.transform.localScale.x * 0.5f * destinationZoneRadius;
                float distanceToZoneCenter = Vector3.Distance(transform.position, destinationZone.transform.position);
                if (distanceToZoneCenter <= zoneRadius)
                {
                    zoneRenderer.material.color = insideColor.value;
                }
                else
                {
                    zoneRenderer.material.color = outsideColor;
                }
                destinationZone.SetActive(true);
            }
            else
            {
                destinationZone.SetActive(false);
            }
        }

        // ! DEBUG

        public void Debug_SetDestinationPlanet(PlanetBehaviour toSet)
        {
            StartLookingForDestination(false);
            SelectNewDestination(toSet);
            OnDestinationSelected();
        }
    }
}