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
            }

            if (gameSnapshot.loops == null)
                return;

            foreach (LoopData loop in gameSnapshot.loops)
            {
                if (!loop.HasDestination)
                    continue;

                PlanetBehaviour destination = planetsSpawner.GetById(loop.destinationId);
                ApplyDestinationRouteColor(destination, loop.colorCode, spaceshipsSpawner);
            }
        }

        public static void SyncHubRouteColors(
            GameSnapshot gameSnapshot,
            HubsSpawner hubsSpawner,
            SpaceshipsSpawner spaceshipsSpawner)
        {
            foreach (HubBehaviour hub in hubsSpawner.All)
            {
                if (!hub.IsActive)
                    continue;

                hub.ResetRouteColor();
            }

            if (gameSnapshot.loops == null)
                return;

            foreach (LoopData loop in gameSnapshot.loops)
                ApplyHubRouteColor(hubsSpawner.GetById(loop.hubId), loop.colorCode, spaceshipsSpawner);
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
        }
    }
}
