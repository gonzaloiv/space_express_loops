using System.Collections.Generic;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // Bulk operations over the zone views owned by SpaceShapingSpawner. Keeps the per-view
    // calls out of SpaceShapingView.
    public static class SpaceZoneViewExtensions
    {
        public static void ApplyConditions(this IReadOnlyList<SpaceZoneView> zones, ZoneConditions value)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].SetConditions(value);
        }

        public static void ApplyScaleEnvelope(
            this IReadOnlyList<SpaceZoneView> zones, float min, float max)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].SetScaleEnvelope(min, max);
        }

        public static void ApplyPrediction(this IReadOnlyList<SpaceZoneView> zones, ZoneVerdict verdict)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].SetPrediction(verdict);
        }

        public static void ClearPredictions(this IReadOnlyList<SpaceZoneView> zones)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].ClearPrediction();
        }

        public static bool AnyScaling(this IReadOnlyList<SpaceZoneView> zones)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].IsScaling)
                    return true;
            }

            return false;
        }

        public static SpaceZoneView Active(this IReadOnlyList<SpaceZoneView> zones)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].IsScaling)
                    return zones[i];
            }

            return zones.Count > 0 ? zones[0] : null;
        }
    }
}
