using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal static class SectorSeparation
    {
        private const int SeparationPasses = 32;

        public static void Resolve(
            SectorPlanet[] planets, float minSeparation, float innerRadius, float outerRadius)
        {
            float outer = Mathf.Max(innerRadius, outerRadius);
            for (int pass = 0; pass < SeparationPasses; pass++)
            {
                bool moved = false;
                for (int i = 0; i < planets.Length; i++)
                {
                    for (int j = i + 1; j < planets.Length; j++)
                        moved |= PushApart(planets, i, j, minSeparation);
                }

                if (!moved)
                    return;

                for (int i = 0; i < planets.Length; i++)
                    planets[i] = Constrain(planets[i], innerRadius, outer);
            }
        }

        private static bool PushApart(SectorPlanet[] planets, int i, int j, float minSeparation)
        {
            Vector3 delta = planets[j].Position - planets[i].Position;
            float required = minSeparation * (planets[i].Radius + planets[j].Radius);
            float distance = delta.magnitude;
            if (distance >= required)
                return false;

            Vector3 direction = distance > 0.0001f ? delta / distance : Vector3.right;
            float push = (required - distance) * 0.5f;
            planets[i] = new SectorPlanet(planets[i].Position - direction * push, planets[i].Radius);
            planets[j] = new SectorPlanet(planets[j].Position + direction * push, planets[j].Radius);
            return true;
        }

        private static SectorPlanet Constrain(SectorPlanet planet, float inner, float outer)
        {
            float length = planet.Position.magnitude;
            if (length <= 0.0001f)
                return new SectorPlanet(new Vector3(inner, 0f, 0f), planet.Radius);

            float clamped = Mathf.Clamp(length, inner, outer);
            return new SectorPlanet(planet.Position * (clamped / length), planet.Radius);
        }
    }
}
