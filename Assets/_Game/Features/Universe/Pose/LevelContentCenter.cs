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
            Vector3 sum = Vector3.zero;
            int count = 0;

            for (int i = 0; i < hubs.Count; i++)
            {
                sum += view.WorldToContentLocal(hubs[i].Position);
                count++;
            }

            for (int i = 0; i < planets.Count; i++)
            {
                sum += view.WorldToContentLocal(planets[i].Position);
                count++;
            }

            if (count == 0)
                return false;

            view.SetContentCenterLocal(sum / count);
            return true;
        }

    }
}
