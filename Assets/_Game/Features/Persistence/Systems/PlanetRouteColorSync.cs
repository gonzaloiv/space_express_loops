using DigitalLove.Game.Spaceships;
using UnityEngine;
using DigitalLove.Game.Planets;

namespace DigitalLove.Game.Persistence
{
    public static class PlanetRouteColorSync
    {
        public static void Apply(GameSnapshot gameSnapshot, PlanetsSpawner planetsSpawner, SpaceshipsSpawner spaceshipsSpawner)
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

                if (!spaceshipsSpawner.TryGetRouteColor(loop.colorCode, out Color color))
                    continue;

                PlanetBehaviour destination = planetsSpawner.GetById(loop.destinationId);
                destination?.PlanetBody.SetRouteColor(color);
            }
        }

        public static void ApplyDestinationRouteColor(PlanetBehaviour destination, string colorCode, SpaceshipsSpawner spaceshipsSpawner)
        {
            if (destination == null || !spaceshipsSpawner.TryGetRouteColor(colorCode, out Color color))
                return;

            destination.PlanetBody.SetRouteColor(color);
        }
    }
}
