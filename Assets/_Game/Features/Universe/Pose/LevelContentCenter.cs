using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public static class LevelContentCenter
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
                sum += view.WorldToContentLocal(hubs[i].Position);

            for (int i = 0; i < planets.Count; i++)
                sum += view.WorldToContentLocal(planets[i].Position);

            view.SetContentCenterLocal(sum / count);
            return true;
        }
    }
}
