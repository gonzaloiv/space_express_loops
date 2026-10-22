using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Universe;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    internal static class LevelContentCenter
    {
        public static bool TrySet(
            UniverseView view,
            IReadOnlyList<HubBehaviour> hubs,
            IReadOnlyList<PlanetBehaviour> planets)
        {
            int count = hubs.Count + planets.Count;
            if (count == 0)
                return false;

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < hubs.Count; i++)
                sum += hubs[i].Position;
            for (int i = 0; i < planets.Count; i++)
                sum += planets[i].Position;

            view.SetContentCenter(sum / count);
            return true;
        }
    }
}
