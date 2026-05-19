using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.DebugActions
{
    [RequireComponent(typeof(EditorDebugContext))]
    public class SpaceshipSelectionDebugActions : MonoBehaviour
    {
        private EditorDebugContext context;

        private void Awake() => context = GetComponent<EditorDebugContext>();

        [Button]
        public void Debug_StartSelectingDestination()
        {
            SpaceshipBehaviour ship = context.ResolveTargetShip();
            if (ship == null)
                return;

            ship.StartSelectingDestination();
            Debug.Log($"Ship {ship.Id}: selecting destination.");
        }

        [Button]
        public void Debug_SetDestinationOnTargetShip()
        {
            SpaceshipBehaviour ship = context.ResolveTargetShip();
            PlanetBehaviour planet = context.ResolveDestinationPlanet(ship);
            if (ship == null || planet == null)
                return;

            ship.DestinationSelector.Debug_SetDestinationPlanet(planet);
            Debug.Log($"Ship {ship.Id}: destination set to {planet.Id}.");
        }

        [Button]
        public void Debug_ConfirmDestinationOnTargetShip()
        {
            SpaceshipBehaviour ship = context.ResolveTargetShip();
            if (ship == null)
                return;

            ship.Debug_ConfirmDestination();
            Debug.Log($"Ship {ship.Id}: confirmed (stops: {ship.Loop.Destinations.Count}).");
        }

        [Button]
        public void Debug_SetDestinationAndConfirm()
        {
            Debug_SetDestinationOnTargetShip();
            Debug_ConfirmDestinationOnTargetShip();
        }

        [Button]
        public void Debug_AppendRandomDestination()
        {
            SpaceshipBehaviour ship = context.ResolveTargetShip();
            if (ship == null || !ship.HasRoute)
            {
                Debug.LogWarning("EditorDebug: Target ship must be active with a running loop.");
                return;
            }

            PlanetBehaviour planet = context.GetRandomPlanetExcludingShip(ship);
            if (planet == null)
                return;

            ship.StartSelectingDestination();
            ship.DestinationSelector.Debug_SetDestinationPlanet(planet);
            ship.Debug_ConfirmDestination();
            Debug.Log($"Ship {ship.Id}: appended {planet.Id} (total: {ship.Loop.Destinations.Count}).");
        }
    }
}
