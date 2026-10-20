using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Spaceships;
using UnityEngine;

namespace DigitalLove.Game.DebugActions
{
    public class EditorDebugContext : MonoBehaviour
    {
        [SerializeField] private PlanetsSpawner planets;
        [SerializeField] private HubsSpawner hubs;
        [SerializeField] private SpaceshipsSpawner spaceships;

        [Header("Target ship (optional)")]
        [SerializeField] private SpaceshipBehaviour targetSpaceship;

        [Header("Destination pick (optional)")]
        [SerializeField] private PlanetBehaviour destinationPlanet;

        public PlanetsSpawner Planets => planets;
        public HubsSpawner Hubs => hubs;
        public SpaceshipsSpawner Spaceships => spaceships;
        public PlanetBehaviour DestinationPlanet => destinationPlanet;

        public void EnsureSpaceshipsReady() => spaceships.InitializePool();

        public SpaceshipBehaviour ResolveTargetShip()
        {
            if (targetSpaceship != null && targetSpaceship.IsActive)
                return targetSpaceship;

            for (int i = 0; i < spaceships.All.Count; i++)
            {
                SpaceshipBehaviour spaceship = spaceships.All[i];
                if (spaceship != null && spaceship.IsActive)
                    return spaceship;
            }

            Debug.LogWarning("EditorDebug: No active spaceship. Spawn one or assign targetSpaceship.");
            return null;
        }

        public PlanetBehaviour ResolveDestinationPlanet(SpaceshipBehaviour ship)
        {
            if (destinationPlanet != null)
                return destinationPlanet;

            return GetRandomPlanetExcludingShip(ship);
        }

        public PlanetBehaviour GetRandomPlanetExcludingShip(SpaceshipBehaviour ship)
        {
            List<string> excluded = ship.Debug_GetDestinationIds();
            excluded.Add(ship.Hub.Id);
            return GetRandomPlanetExcludingIds(excluded);
        }

        public PlanetBehaviour GetRandomPlanetExcludingIds(IReadOnlyList<string> excluded)
        {
            List<PlanetBehaviour> available = new();
            for (int i = 0; i < planets.All.Count; i++)
            {
                PlanetBehaviour planet = planets.All[i];
                if (planet == null || !planet.IsActive)
                    continue;
                if (excluded != null && Contains(excluded, planet.Id))
                    continue;

                available.Add(planet);
            }

            if (available.Count == 0)
            {
                Debug.LogWarning("EditorDebug: No available planets.");
                return null;
            }

            return available[Random.Range(0, available.Count)];
        }

        private static bool Contains(IReadOnlyList<string> ids, string id)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (string.Equals(ids[i], id))
                    return true;
            }

            return false;
        }

        public SpaceshipBehaviour ResolveShipForRoute() =>
            ResolveTargetShip() ?? Spaceships.GetRandomActive();

        public void PrepareShipForRoute(SpaceshipBehaviour ship)
        {
            if (ship == null || !ship.IsActive)
                return;

            EnsureSpaceshipsReady();
            if (!ship.IsInitialized)
                ship.Initialize();
            spaceships.WireLoopHandlers(ship);
        }
    }
}
