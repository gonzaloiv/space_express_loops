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

            Vector3 center = sum / count;
            view.SetContentCenter(center);
            view.SetContentRadius(MaxDistanceFrom(center, hubs, planets));
            return true;
        }

        private static float MaxDistanceFrom(
            Vector3 center,
            IReadOnlyList<HubBehaviour> hubs,
            IReadOnlyList<PlanetBehaviour> planets)
        {
            float radius = 0f;
            for (int i = 0; i < hubs.Count; i++)
                radius = Mathf.Max(radius, (hubs[i].Position - center).magnitude);
            for (int i = 0; i < planets.Count; i++)
                radius = Mathf.Max(radius, (planets[i].Position - center).magnitude);

            return radius;
        }
    }
}
