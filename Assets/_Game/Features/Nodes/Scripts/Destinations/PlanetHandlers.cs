using System;

namespace DigitalLove.Game.Nodes
{
    public readonly struct PlanetHandlers
    {
        public Action OnPlanetFull { get; }

        public PlanetHandlers(Action onPlanetFull = null)
        {
            OnPlanetFull = onPlanetFull;
        }
    }
}
