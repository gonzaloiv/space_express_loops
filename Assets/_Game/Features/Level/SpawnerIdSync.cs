using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.Spaceships;

namespace DigitalLove.Game.Levels
{
    public static class SpawnerIdSync
    {
        public static void Sync(
            GameSnapshot gameSnapshot,
            PlanetsSpawner planetsSpawner,
            SpaceshipsSpawner spaceshipsSpawner,
            HubsSpawner hubsSpawner)
        {
            List<string> planetIds = new(gameSnapshot.planets.Count);
            foreach (PlanetData planet in gameSnapshot.planets)
                planetIds.Add(planet.id);
            planetsSpawner.SyncIdsFromSnapshot(planetIds);

            List<string> spaceshipIds = new(gameSnapshot.loops.Count);
            foreach (LoopData loop in gameSnapshot.loops)
                spaceshipIds.Add(loop.spaceshipId);
            spaceshipsSpawner.SyncIdsFromSnapshot(spaceshipIds);

            List<string> hubIds = new(gameSnapshot.hubs.Count);
            foreach (HubData hub in gameSnapshot.hubs)
                hubIds.Add(hub.id);
            hubsSpawner.SyncIdsFromSnapshot(hubIds);
        }
    }
}
