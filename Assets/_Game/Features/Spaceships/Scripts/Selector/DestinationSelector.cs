using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class DestinationSelector : MonoBehaviour
    {
        [SerializeField] private RaycastHelper raycastHelper;
        [SerializeField] private DestinationZone destinationZone;

        [Header("Timing")]
        [SerializeField] private float secsToSelect = 0.5f;
        [SerializeField] private float initialSecsToSelect = 0.2f;

        private float countdown;
        private bool isLookingForDestination;

        private HubBehaviour hub;
        private readonly HashSet<string> excludedPlanetIds = new();
        private PlanetBehaviour destinationPlanet;

        public HubBehaviour Hub => hub;
        public PlanetBehaviour Destination => destinationPlanet;
        public bool HasDestinationBeenSelected => destinationPlanet != null && destinationPlanet.IsDestination;
        public bool IsLookingForDestination => isLookingForDestination;

        public void StartLookingForDestination(bool isLookingForDestination)
        {
            this.isLookingForDestination = isLookingForDestination;
            raycastHelper.SetActive(isLookingForDestination);
        }

        public void Init(HubBehaviour hub, Color color)
        {
            this.hub = hub;
            raycastHelper.SetColor(color);
            destinationZone.SetColor(color);
            excludedPlanetIds.Clear();
        }

        public void SetExcludedPlanetIds(IEnumerable<string> planetIds)
        {
            excludedPlanetIds.Clear();
            if (planetIds == null)
                return;

            foreach (string id in planetIds)
            {
                if (!string.IsNullOrEmpty(id))
                    excludedPlanetIds.Add(id);
            }
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
                if (candidatePlanet != destinationPlanet && IsSelectableOffRoute(candidatePlanet))
                    SelectNewDestination(candidatePlanet);
            }
        }

        private bool IsSelectableOffRoute(PlanetBehaviour planet) =>
            PlanetSelectionRules.IsSelectable(planet, hub, excludedPlanetIds);

        private void DeselectCurrent()
        {
            destinationPlanet.ApplyRouteVisual(RouteVisualState.Default);
            destinationPlanet = null;
        }

        private void SelectNewDestination(PlanetBehaviour newDestination)
        {
            if (destinationPlanet != null)
                destinationPlanet.ApplyRouteVisual(RouteVisualState.Default);
            destinationPlanet = newDestination;
            destinationPlanet.ApplyRouteVisual(RouteVisualState.SelectingCandidate);
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
            destinationPlanet.ApplyRouteVisual(RouteVisualState.ConfirmedDestination);
        }

        private void UpdateDestinationZone()
        {
            bool isActive = isLookingForDestination && destinationPlanet != null && countdown <= secsToSelect;
            if (isActive)
                destinationZone.DoUpdate(countdown, secsToSelect, transform.position, destinationPlanet);
            destinationZone.SetActive(isActive);
        }

        #region Debug

        public void Debug_SetDestinationPlanet(PlanetBehaviour toSet)
        {
            StartLookingForDestination(false);
            SelectNewDestination(toSet);
            OnDestinationSelected();
        }

        #endregion
    }
}
