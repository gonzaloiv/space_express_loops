using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Spaceships;

namespace DigitalLove.Game.Levels
{
    public sealed class LevelLoopPlanetAvailability : ILoopPlanetAvailability
    {
        private readonly PlanetsSpawner planetsSpawner;
        private readonly HubsSpawner hubsSpawner;

        public LevelLoopPlanetAvailability(PlanetsSpawner planetsSpawner, HubsSpawner hubsSpawner)
        {
            this.planetsSpawner = planetsSpawner;
            this.hubsSpawner = hubsSpawner;
        }

        public bool HasSelectableOffRoute(string hubId, IReadOnlyList<string> destinationIds) =>
            planetsSpawner.HasAnySelectableForHub(hubsSpawner.GetById(hubId), destinationIds);
    }
}