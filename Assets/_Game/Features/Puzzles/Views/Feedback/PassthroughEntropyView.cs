using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Ambient entropy cue on the real room: passthrough desaturates and dims as the budget drains,
    // with a short pulse when a commit crosses a star band. The in-field bar remains the meter.
    public class PassthroughEntropyView : MonoBehaviour
    {
        private const float Epsilon = 0.0001f;

        [SerializeField] private SpaceShapingView view;
        [SerializeField] private OVRPassthroughLayer passthrough;
        [Tooltip("Remaining fraction where the room starts to strain. Default: the last third.")]
        [SerializeField] private float stressStartRemaining = 1f / 3f;
        [SerializeField] private float maxSaturationDrop = 0.3f;
        [SerializeField] private float maxOpacityDrop = 0.15f;
        [SerializeField] private float easeSecs = 0.35f;
        [SerializeField] private float flashSecs = 0.3f;
        [SerializeField] private float flashStrength = 0.25f;

        private float targetStress;
        private float stress;
        private float flash;
        private float appliedStrain = -1f;
        private float lastSpent;
        private float lastBudget;
        private float baselineBrightness;
        private float baselineContrast;
        private float baselineSaturation;
        private float baselineOpacity;

        private void OnEnable()
        {
            view.ReadoutChanged += OnReadoutChanged;
            baselineBrightness = passthrough.colorMapEditorBrightness;
            baselineContrast = passthrough.colorMapEditorContrast;
            baselineSaturation = passthrough.colorMapEditorSaturation;
            baselineOpacity = passthrough.textureOpacity;

            PuzzleReadout readout = view.Readout;
            lastSpent = readout.EntropySpent;
            lastBudget = readout.EntropyBudget;
            targetStress = Stress(readout);
            stress = targetStress;
            flash = 0f;
            appliedStrain = -1f;
        }

        private void Start() => Apply();

        private void OnDisable()
        {
            view.ReadoutChanged -= OnReadoutChanged;
            stress = 0f;
            flash = 0f;
            appliedStrain = -1f;
            if (!passthrough.isActiveAndEnabled)
                return;

            passthrough.SetBrightnessContrastSaturation(
                baselineBrightness, baselineContrast, baselineSaturation);
            passthrough.textureOpacity = baselineOpacity;
        }

        private void Update()
        {
            if (Mathf.Abs(stress - targetStress) > Epsilon)
                stress = Mathf.MoveTowards(stress, targetStress, Delta(easeSecs));

            if (flash > 0f)
                flash = Mathf.MoveTowards(flash, 0f, Delta(flashSecs));

            Apply();
        }

        private void OnReadoutChanged(PuzzleReadout readout)
        {
            targetStress = Stress(readout);
            if (CrossedBand(readout))
                flash = 1f;

            lastSpent = readout.EntropySpent;
            lastBudget = readout.EntropyBudget;
        }

        private bool CrossedBand(PuzzleReadout readout)
        {
            return readout.EntropySpent > lastSpent
                && Band(readout.EntropySpent, readout.EntropyBudget) < Band(lastSpent, lastBudget);
        }

        private static int Band(float spent, float budget) =>
            budget > 0f ? PuzzleGrade.Stars(spent, budget) : 0;

        private float Stress(PuzzleReadout readout) =>
            Mathf.InverseLerp(stressStartRemaining, 0f, EntropyBar.From(readout).Remaining);

        private void Apply()
        {
            if (!passthrough.isActiveAndEnabled)
                return;

            float strain = Mathf.Clamp01(stress + flash * flashStrength);
            if (Mathf.Abs(strain - appliedStrain) < Epsilon)
                return;

            appliedStrain = strain;
            passthrough.SetBrightnessContrastSaturation(
                baselineBrightness, baselineContrast, baselineSaturation - maxSaturationDrop * strain);
            passthrough.textureOpacity = baselineOpacity - maxOpacityDrop * strain;
        }

        private static float Delta(float secs) => secs > 0f ? Time.deltaTime / secs : 1f;
    }
}
