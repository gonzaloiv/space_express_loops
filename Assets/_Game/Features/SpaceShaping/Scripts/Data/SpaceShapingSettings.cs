using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    [CreateAssetMenu(fileName = "SpaceShapingSettings", menuName = "DigitalLove/Game/SpaceShaping Settings")]
    public class SpaceShapingSettings : ScriptableObject
    {
        [Header("Puzzle")]
        [Tooltip("The run: origin to destination, in metres. It also sizes the field, so keep planetRadius well under runDistance / 2.")]
        public float runDistance = 1f;

        [Tooltip("Fallback cube size, in metres — used only when a puzzle has no baked size. Per-level sizes come from the seed's authored range.")]
        public float cubeSize = 0.25f;

        [Header("Simulation")]
        [Tooltip("Integrator step, in seconds. The step budget is derived from the run and the nominal speed, so the budget can never drift from the run.")]
        public float stepSecs = 0.0008333333f;

        [Tooltip("Nominal crossing speed, in m/s. The time budget is runDistance / nominalSpeed, and the baked launch is a fraction of it, so the bare start just misses however long the run is.")]
        public float nominalSpeed = 1f;

        [Header("Field")]
        [Tooltip("Planet radius, in metres. Sets the field planets (with a small spread) and the arrival sphere around the destination, so touching the goal planet is an arrival.")]
        public float planetRadius = 0.06f;

        [Tooltip("Planet gravity per unit volume (mass = radius cubed). Tuned for the toy scale; a grown planet deepens its well fast.")]
        public float sourceStrengthScale = 5.5555553f;

        [Tooltip("Planet spacing, as a multiple of the summed radii. The field builder spaces the sector 1.1x wider so the first constraint pass never moves a baked planet.")]
        public float separationScale = 1.2f;

        [Header("Time")]
        [Tooltip("Toy-scale light speed, in m/s. The clock uses 2*Phi/c^2 - v^2/c^2; real c makes both terms vanish at this scale.")]
        public float lightSpeed = 1.4142135f;

        [Header("Zone")]
        [Tooltip("The cube's soft edge, as a fraction of its half-size (0 = hard edge). Shared with the view so a found solution reproduces.")]
        public float zoneFalloff = 0.25f;

        [Header("Reachable scale")]
        [Tooltip("Smallest scale the player can reach (0.6 = shrink to 60%).")]
        public float minScaleFactor = 0.6f;

        [Tooltip("Largest scale the player can reach (1.8 = magnify 1.8x).")]
        public float maxScaleFactor = 1.8f;
    }
}
