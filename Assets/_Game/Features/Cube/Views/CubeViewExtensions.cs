using System.Collections.Generic;
using DigitalLove.Game.Spatial;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // Bulk operations over the zone views owned by the zone pool. Keeps per-view calls out of
    // SpaceShapingView.
    public static class CubeViewExtensions
    {
        public static void ApplyConditions(this IReadOnlyList<CubeView> zones, ZoneConditions value)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].SetConditions(value);
        }

        public static void ApplyScaleEnvelope(
            this IReadOnlyList<CubeView> zones, float min, float max)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].SetScaleEnvelope(min, max);
        }

        public static void ApplyPrediction(this IReadOnlyList<CubeView> zones, ZoneVerdict verdict)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].SetPrediction(verdict);
        }

        public static void ClearPredictions(this IReadOnlyList<CubeView> zones)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].ClearPrediction();
        }

        public static void ApplyStrain(this IReadOnlyList<CubeView> zones, float strain)
        {
            for (int i = 0; i < zones.Count; i++)
                zones[i].SetStrain(strain);
        }

        public static bool AnyScaling(this IReadOnlyList<CubeView> zones)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].IsScaling)
                    return true;
            }

            return false;
        }

        public static CubeMoveRefusal FirstMoveRefusal(this IReadOnlyList<CubeView> zones)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                CubeView zone = zones[i];
                if (!zone.gameObject.activeSelf)
                    continue;

                CubeMoveRefusal refusal = zone.MoveRefusal;
                if (refusal.Blocked)
                    return refusal;
            }

            return default;
        }


        public static bool AnyHeld(this IReadOnlyList<CubeView> zones)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].isActiveAndEnabled && zones[i].IsHeld)
                    return true;
            }

            return false;
        }

        public static CubeView Active(this IReadOnlyList<CubeView> zones)
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
