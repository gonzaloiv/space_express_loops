using System.Collections.Generic;

namespace DigitalLove.Game.Nodes
{
    public static class PlanetSelectionRules
    {
        public static bool IsSelectable(
            PlanetBehaviour planet,
            HubBehaviour hub,
            HashSet<string> excludedPlanetIds) =>
            planet != null
            && planet.IsActive
            && hub != null
            && planet.transform != hub.transform
            && !excludedPlanetIds.Contains(planet.Id)
            && !planet.IsOnRoute;

        public static HashSet<string> ToExcludedSet(IEnumerable<string> excludedPlanetIds)
        {
            HashSet<string> excluded = new();
            if (excludedPlanetIds == null)
                return excluded;

            foreach (string id in excludedPlanetIds)
            {
                if (!string.IsNullOrEmpty(id))
                    excluded.Add(id);
            }

            return excluded;
        }
    }
}
