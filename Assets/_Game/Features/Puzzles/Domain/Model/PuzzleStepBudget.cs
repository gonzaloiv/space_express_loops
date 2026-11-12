using System.Collections.Generic;
using DigitalLove.Game.Spatial;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The integrator's step budget: long enough for the run at the nominal speed, widened by the
    // strongest active zone. Shared by the runtime session and the bake so they cannot drift.
    public static class PuzzleStepBudget
    {
        public static int Steps(float run, PuzzleSimSettings settings, float zoneFactor)
        {
            if (settings.NominalSpeed <= 0f || settings.StepSecs <= 0f)
                return 1;

            return Mathf.Max(
                1, Mathf.CeilToInt(run / settings.NominalSpeed / settings.StepSecs * zoneFactor));
        }

        public static int Scaled(int steps, float zoneFactor) =>
            Mathf.CeilToInt(steps * zoneFactor);

        public static float ZoneFactor(IReadOnlyList<SpaceZone> zones)
        {
            float factor = 1f;
            if (zones == null)
                return factor;

            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].IsActive)
                    factor = Mathf.Max(factor, zones[i].Scale);
            }

            return factor;
        }
    }
}
