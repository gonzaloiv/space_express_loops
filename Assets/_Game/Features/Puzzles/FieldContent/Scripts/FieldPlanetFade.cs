using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The map's distance fade for one field planet view. Shared by the sun, the sources and the
    // endpoint pools so every planet fades by the same rule.
    public static class FieldPlanetFade
    {
        public static void Apply(
            FieldPlanetBehaviour planet, Vector3 focus, float near, float far)
        {
            if (planet == null || !planet.gameObject.activeSelf)
                return;

            float distance = Vector3.Distance(planet.transform.position, focus);
            if (distance >= far)
            {
                planet.SetCulled(true);
                planet.SetFade(1f);
                return;
            }

            planet.SetCulled(false);
            planet.SetFade(Mathf.InverseLerp(near, far, distance));
        }

        public static void Clear(FieldPlanetBehaviour planet)
        {
            if (planet == null || !planet.gameObject.activeSelf)
                return;

            planet.SetCulled(false);
            planet.SetFade(0f);
        }
    }
}
