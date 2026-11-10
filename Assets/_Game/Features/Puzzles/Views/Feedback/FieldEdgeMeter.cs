using DigitalLove.Global;
using DigitalLove.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace DigitalLove.Game.SpaceShaping
{
    // The entropy budget drawn on the grid's base edge. Two arms run from the front-centre around both
    // sides to the back-centre; spending cuts both from the front, and ticks mark the star bands. The
    // arms and ticks are authored line renderers; this only sets their points and colours.
    public class FieldEdgeMeter : MonoBehaviour
    {
        [SerializeField] private SpaceShapingView view;
        [SerializeField] private FieldEdgeMeterView drawing;
        [SerializeField] private ColorValue accent;
        [SerializeField] private ColorValue neutral;
        [SerializeField] private ColorValue danger;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform labelRoot;
        [SerializeField] private string entropyKey = "field_meter_entropy";
        [SerializeField] private string overBudgetKey = "field_meter_over_budget";
        [SerializeField, Range(0f, 1f)] private float spentAlpha = 0.12f;
        [SerializeField, Min(0f)] private float labelHeight = 0.012f;
        [SerializeField, Min(0f)] private float labelOutset = 0.008f;

        private readonly GradientColorKey[] colorKeys = new GradientColorKey[4];
        private readonly GradientAlphaKey[] alphaKeys = new GradientAlphaKey[4];
        private Gradient gradient;
        private Vector3 laidCentre;
        private float laidHalf = -1f;
        private bool? labelOverBudget;

        private Gradient Gradient => gradient ??= new Gradient { mode = GradientMode.Fixed };

        private void OnEnable()
        {
            view.ReadoutChanged += Render;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            laidHalf = -1f;
            Render(view.Readout);
        }

        private void OnDisable()
        {
            view.ReadoutChanged -= Render;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale locale) => RefreshLabel();

        private void RefreshLabel()
        {
            if (!labelOverBudget.HasValue)
                return;

            bool overBudget = labelOverBudget.Value;
            labelOverBudget = null;
            SetLabel(overBudget);
        }

        private void LateUpdate()
        {
            if (view.FieldHalf == laidHalf && view.FieldCentre == laidCentre)
                return;

            laidCentre = view.FieldCentre;
            laidHalf = view.FieldHalf;
            drawing.Layout(laidCentre, laidHalf, 2);
            labelRoot.localPosition = new Vector3(0f, -laidHalf + labelHeight, -laidHalf - labelOutset);
        }

        // Along each arm: spent (dim), the last edit, the in-flight preview, then what remains.
        private void Render(PuzzleReadout readout)
        {
            EntropyBar bar = EntropyBar.From(readout);
            float spent = bar.Committed;
            float preview = Mathf.Min(spent + bar.Pending, 1f);
            SetKey(0, Mathf.Max(spent - bar.LastEdit, 0f), neutral.value, spentAlpha);
            SetKey(1, spent, neutral.value, 1f);
            SetKey(2, preview, bar.Overflow ? danger.value : accent.value, 1f);
            SetKey(3, 1f, accent.value, 1f);
            Gradient.SetKeys(colorKeys, alphaKeys);
            drawing.SetGradient(Gradient);
            SetLabel(bar.Overflow);
        }

        private void SetLabel(bool overBudget)
        {
            if (labelOverBudget == overBudget)
                return;

            labelOverBudget = overBudget;
            label.text = LocalizationUtil.GetValue(overBudget ? overBudgetKey : entropyKey);
            label.color = overBudget ? danger.value : neutral.value;
        }

        private void SetKey(int index, float time, Color color, float alpha)
        {
            colorKeys[index] = new GradientColorKey(color, time);
            alphaKeys[index] = new GradientAlphaKey(alpha, time);
        }
    }
}
