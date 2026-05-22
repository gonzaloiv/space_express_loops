using System.Collections.Generic;
using DigitalLove.Game.Spaceships;
using UnityEngine;
using DigitalLove.Game.Nodes;

namespace DigitalLove.Game.Persistence
{
    public static class PlanetRouteColorSync
    {
        public static void Apply(GameSnapshot gameSnapshot,
            PlanetsSpawner planetsSpawner,
            HubsSpawner hubsSpawner,
            SpaceshipsSpawner spaceshipsSpawner)
        {
            SyncPlanetRouteColors(gameSnapshot, planetsSpawner, spaceshipsSpawner);
            SyncHubRouteColors(gameSnapshot, hubsSpawner, spaceshipsSpawner);
        }

        public static void SyncPlanetRouteColors(
            GameSnapshot gameSnapshot,
            PlanetsSpawner planetsSpawner,
            SpaceshipsSpawner spaceshipsSpawner)
        {
            foreach (PlanetBehaviour planet in planetsSpawner.All)
            {
                if (!planet.IsActive)
                    continue;

                planet.ResetRouteColor();
            }

            if (gameSnapshot.loops == null)
                return;

            foreach (LoopData loop in gameSnapshot.loops)
            {
                if (!loop.HasDestinations)
                    continue;

                foreach (string destinationId in loop.destinationIds)
                {
                    PlanetBehaviour destination = planetsSpawner.GetById(destinationId);
                    ApplyRouteColor(destination, loop.colorCode, spaceshipsSpawner);
                }
            }
        }

        public static void SyncHubRouteColors(
            GameSnapshot gameSnapshot,
            HubsSpawner hubsSpawner,
            SpaceshipsSpawner spaceshipsSpawner)
        {
            HashSet<string> hubIdsOnLoops = new();

            if (gameSnapshot.loops != null)
            {
                foreach (LoopData loop in gameSnapshot.loops)
                {
                    IRouteWorldNode hub = ResolveHub(loop, hubsSpawner, spaceshipsSpawner);
                    if (hub == null)
                        continue;

                    hubIdsOnLoops.Add(hub.Id);
                    ApplyRouteColor(hub, loop.colorCode, spaceshipsSpawner);
                }
            }

            foreach (HubBehaviour hub in hubsSpawner.All)
            {
                if (!hub.IsActive || hubIdsOnLoops.Contains(hub.Id))
                    continue;

                hub.ResetRouteColor();
            }
        }

        private static void ApplyRouteColor(
            IRouteWorldNode node,
            string colorCode,
            SpaceshipsSpawner spaceshipsSpawner)
        {
            if (node == null || !spaceshipsSpawner.TryGetRouteColor(colorCode, out Color color))
                return;

            node.ApplyRouteColor(color);
        }

        private static IRouteWorldNode ResolveHub(
            LoopData loop,
            HubsSpawner hubsSpawner,
            SpaceshipsSpawner spaceshipsSpawner)
        {
            if (!string.IsNullOrEmpty(loop.hubId))
            {
                HubBehaviour hub = hubsSpawner.GetById(loop.hubId);
                if (hub != null)
                    return hub;
            }

            SpaceshipBehaviour spaceship = spaceshipsSpawner.GetActiveById(loop.spaceshipId);
            return spaceship?.Hub;
        }
    }
}
