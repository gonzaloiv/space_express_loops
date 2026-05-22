using System.Collections.Generic;

namespace DigitalLove.Game.Spaceships
{
    public interface ILoopPlanetAvailability
    {
        bool HasSelectableOffRoute(string hubId, IReadOnlyList<string> destinationIds);
    }
}
