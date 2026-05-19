using System.Collections.Generic;
using System.Linq;
using DigitalLove.Game.Planets;
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

            SpaceshipBehaviour active = spaceships.All.FirstOrDefault(s => s.IsActive);
            if (active != null)
                return active;

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
            List<string> excluded = ship.Loop.GetDestinationIds();
            if (ship.Hub != null)
                excluded.Add(ship.Hub.Id);
            return GetRandomPlanetExcludingIds(excluded);
        }

        public PlanetBehaviour GetRandomPlanetExcludingIds(IReadOnlyList<string> excluded)
        {
            List<PlanetBehaviour> available = planets.All
                .Where(p => p != null && p.IsActive && (excluded == null || !excluded.Contains(p.Id)))
                .ToList();

            if (available.Count == 0)
            {
                Debug.LogWarning("EditorDebug: No available planets.");
                return null;
            }

            return available[Random.Range(0, available.Count)];
        }

        public SpaceshipBehaviour ResolveShipForRoute()
        {
            SpaceshipBehaviour ship = ResolveTargetShip() ?? Spaceships.GetRandomActive();
            if (ship == null)
                Debug.LogWarning("EditorDebug: No active spaceship. Spawn one or assign targetSpaceship.");

            return ship;
        }

        public void PrepareShipForRoute(SpaceshipBehaviour ship)
        {
            if (ship == null || !ship.IsActive)
                return;

            EnsureSpaceshipsReady();
            if (!ship.IsInitialized)
                ship.Initialize();
            WireLoopLoggers(ship);
        }

        public static void WireLoopLoggers(SpaceshipBehaviour ship)
        {
            ship.SetOnLoopChanged(args =>
                Debug.Log($"[Loop] {args.spaceshipId} destinations: {string.Join(" → ", args.destinationIds)}"));
            ship.SetOnLoopComplete(args =>
                Debug.Log($"[Loop complete] {args.spaceshipId} value={args.value}"));
            ship.SetOnLoopEditionButtonClicked(args =>
                Debug.Log($"[Loop edit] {args.spaceshipId} cleared route."));
        }
    }
}
