using DigitalLove.Global;
using DigitalLove.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace DigitalLove.Game.SpaceShaping
{
    // The entropy budget as a post on the grid's back-left edge. Spending cuts it from the top; a
    // pinch shows its cost below the top before it commits; a star beside each band holds, flickers
    // when the pinch would cost it, and goes dark once spent past.
    public class FieldEdgeMeter : MonoBehaviour
    {
        // Top to bottom, matching the drawing's stars: 3 stars, 2 stars, 1 star (the whole budget).
        private static readonly float[] StarLimits =
            { PuzzleGrade.ThreeStarUse, PuzzleGrade.TwoStarUse, 1f };

        [SerializeField] private FieldFeedbackSource view;
        [SerializeField] private FieldEdgeMeterView drawing;
        [SerializeField] private ColorValue accent;
        [SerializeField] private ColorValue neutral;
        [SerializeField] private ColorValue danger;
        [Tooltip("The stars beside the post, the same colour as every star in the game.")]
        [SerializeField] private ColorValue star;
        [SerializeField] private TMP_Text label;
        [Tooltip("The job's id above the post's label, so the field says which contract it is.")]
        [SerializeField] private TMP_Text jobLabel;
        [SerializeField] private Transform labelRoot;
        [SerializeField] private string entropyKey = "field_meter_entropy";
        [SerializeField] private string jobKey = "field_meter_job";
        [SerializeField] private string overBudgetKey = "field_meter_over_budget";
        [SerializeField, Range(0f, 1f)] private float spentAlpha = 0.12f;
        [SerializeField, Range(0f, 1f)] private float lastEditAlpha = 0.45f;
        [SerializeField, Range(0f, 1f)] private float lostStarAlpha = 0.15f;
        [SerializeField, Min(0.1f)] private float riskFlickerHz = 4f;
        [SerializeField, Min(0f)] private float labelHeight = 0.004f;
        [SerializeField, Min(0f)] private float labelOutset = 0.02f;
        [Tooltip("Gap between the post and the labels' left edge (field metres).")]
        [SerializeField, Min(0f)] private float labelGap = 0.008f;

        private readonly float[] starHeights = new float[3];
        private EntropyPost reading = new(0f, 0f, 0f, false);
        private bool? labelOverBudget;
        private string jobId = string.Empty;

        private void Awake()
        {
            for (int i = 0; i < StarLimits.Length; i++)
                starHeights[i] = EntropyPost.HeightOf(StarLimits[i]);
        }

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

        private void OnLocaleChanged(Locale locale)
        {
            RefreshLabel();
            ShowJob(jobId);
        }

        // "01_05" reads as "JOB 01-05": the chapter and the job, as the contract numbers them.
        public void ShowJob(string id)
        {
            jobId = id ?? string.Empty;
            jobLabel.text = jobId.Length > 0
                ? LocalizationUtil.GetValue(jobKey, jobId.Replace('_', '-'))
                : string.Empty;
        }

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
            float half = view.FieldHalf;
            drawing.Layout(view.FieldCentre, half, starHeights);
            // The labels start at the post's foot and read to its right, along the back edge. They are
            // a child of the drawing, which already sits at the field's centre.
            labelRoot.localPosition = FieldFrame.OnMeterEdge(half, -half + labelHeight, labelOutset)
                + Vector3.right * labelGap;
            TintStars();
        }

        // Up the post: what remains, the pinch's cost, the last edit, then what is spent.
        private void Render(PuzzleReadout readout)
        {
            EntropyBar bar = EntropyBar.From(readout);
            reading = new EntropyPost(bar.Committed, bar.Pending, bar.LastEdit, bar.Overflow);
            drawing.SetBands(new MeterBands(
                new MeterBand(reading.PreviewFrom, accent.value, 1f),
                new MeterBand(reading.RemainingTop, bar.Overflow ? danger.value : neutral.value, 1f),
                new MeterBand(reading.LastEditTop, neutral.value, lastEditAlpha),
                new MeterBand(1f, neutral.value, spentAlpha)));
            SetLabel(bar.Overflow);
        }

        private void TintStars()
        {
            float wave = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * riskFlickerHz * Mathf.PI * 2f);
            for (int i = 0; i < StarLimits.Length; i++)
                drawing.SetStar(i, StarColor(reading.Star(StarLimits[i]), wave));
        }

        // The reset readout drains the post to empty and refills it; the real readout returns on ClearReset.
        public void ShowReset(float progress)
        {
            reading = new EntropyPost(1f - Mathf.Clamp01(progress), 0f, 0f, false);
            drawing.SetBands(new MeterBands(
                new MeterBand(reading.PreviewFrom, accent.value, 0f),
                new MeterBand(reading.RemainingTop, neutral.value, 1f),
                new MeterBand(reading.LastEditTop, neutral.value, 0f),
                new MeterBand(1f, neutral.value, spentAlpha)));
            TintStars();
        }

        public void ClearReset() => Render(view.Readout);

        private Color StarColor(StarHold hold, float wave)
        {
            switch (hold)
            {
                case StarHold.Held:
                    return star.value;
                case StarHold.AtRisk:
                    return Color.Lerp(danger.value, star.value, wave);
                default:
                    return WithAlpha(neutral.value, lostStarAlpha);
            }
        }

        private static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);

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
