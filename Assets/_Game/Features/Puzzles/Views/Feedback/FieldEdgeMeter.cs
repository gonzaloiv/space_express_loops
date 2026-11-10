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

        private bool? labelOverBudget;

        private void OnEnable()
        {
            view.ReadoutChanged += Render;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
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
            drawing.Layout(
                view.FieldCentre, view.FieldHalf, 2, PuzzleGrade.ThreeStarUse, PuzzleGrade.TwoStarUse);
            labelRoot.localPosition = new Vector3(
                0f, -view.FieldHalf + labelHeight, -view.FieldHalf - labelOutset);
        }

        // Along each arm: spent (dim), the last edit, the in-flight preview, then what remains.
        private void Render(PuzzleReadout readout)
        {
            EntropyBar bar = EntropyBar.From(readout);
            float spent = bar.Committed;
            float preview = Mathf.Min(spent + bar.Pending, 1f);
            drawing.SetBands(new MeterBands(
                new MeterBand(Mathf.Max(spent - bar.LastEdit, 0f), neutral.value, spentAlpha),
                new MeterBand(spent, neutral.value, 1f),
                new MeterBand(preview, bar.Overflow ? danger.value : accent.value, 1f),
                new MeterBand(1f, accent.value, 1f)));
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
    }
}
