using DigitalLove.Game.Planets;
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

        public void Init(PlanetBaseBehaviour basePlanet, Color color)
        {
            this.basePlanet = basePlanet;
            raycastHelper.SetColor(color);
            destinationZone.SetColor(color);
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
            bool isActive = isLookingForDestination && destinationPlanet != null && countdown <= secsToSelect;
            if (isActive)
                destinationZone.DoUpdate(countdown, secsToSelect, destinationPlanet.Position);
            destinationZone.SetActive(isActive);
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