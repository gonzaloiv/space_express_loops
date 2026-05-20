using System.Collections.Generic;
using DigitalLove.Game.Planets;

namespace DigitalLove.Game.Spaceships
{
    public interface IRouteLoopBuilder
    {
        void Build(HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations, List<RouteLegPath> legs);
    }
}
