using System.Collections.Generic;
using DigitalLove.Game.Spaceships;
using UnityEngine;
using DigitalLove.Game.Planets;

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

        /// <summary>
        /// Resets every active planet to default, then tints only planets that are a confirmed loop destination.
        /// </summary>
        public static void SyncPlanetRouteColors(
            GameSnapshot gameSnapshot,
            PlanetsSpawner planetsSpawner,
            SpaceshipsSpawner spaceshipsSpawner)
        {
            foreach (PlanetBehaviour planet in planetsSpawner.All)
            {
                if (!planet.IsActive)
                    continue;

                planet.PlanetBody.ResetRouteColor();
                planet.SetOnRoute(false);
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
                    ApplyDestinationRouteColor(destination, loop.colorCode, spaceshipsSpawner);
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
                    HubBehaviour hub = ResolveHub(loop, hubsSpawner, spaceshipsSpawner);
                    if (hub == null)
                        continue;

                    hubIdsOnLoops.Add(hub.Id);
                    ApplyHubRouteColor(hub, loop.colorCode, spaceshipsSpawner);
                }
            }

            foreach (HubBehaviour hub in hubsSpawner.All)
            {
                if (!hub.IsActive || hubIdsOnLoops.Contains(hub.Id))
                    continue;

                hub.ResetRouteColor();
            }
        }

        public static void ApplyHubRouteColor(HubBehaviour hub, string colorCode, SpaceshipsSpawner spaceshipsSpawner)
        {
            if (hub == null || !spaceshipsSpawner.TryGetRouteColor(colorCode, out Color color))
                return;

            hub.SetRouteColor(color);
        }

        public static void ApplyDestinationRouteColor(PlanetBehaviour destination, string colorCode, SpaceshipsSpawner spaceshipsSpawner)
        {
            if (destination == null || !spaceshipsSpawner.TryGetRouteColor(colorCode, out Color color))
                return;

            destination.PlanetBody.SetRouteColor(color);
            destination.SetOnRoute(true);
        }

        private static HubBehaviour ResolveHub(
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
