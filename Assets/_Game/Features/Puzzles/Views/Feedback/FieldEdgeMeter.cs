using DigitalLove.Global;
using DigitalLove.Localization;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The entropy budget drawn on the grid's base edge. Two arms run from the front-centre around both
    // sides to the back-centre; spending cuts both from the front, and ticks mark the star bands. The
    // arms and ticks are authored line renderers; this only sets their points and colours.
    public class FieldEdgeMeter : MonoBehaviour
    {
        // 96 segments per arm, so both corners (s = 0.25 and 0.75) land on a vertex.
        private const int Points = 97;

        [SerializeField] private SpaceShapingView view;
        [SerializeField] private LineRenderer leftArm;
        [SerializeField] private LineRenderer rightArm;
        [Tooltip("Left then right tick at each star band: 3★ left, 3★ right, 2★ left, 2★ right.")]
        [SerializeField] private LineRenderer[] ticks;
        [SerializeField] private ColorValue accent;
        [SerializeField] private ColorValue neutral;
        [SerializeField] private ColorValue danger;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform labelRoot;
        [SerializeField] private string entropyKey = "field_meter_entropy";
        [SerializeField] private string overBudgetKey = "field_meter_over_budget";
        [SerializeField, Range(0f, 1f)] private float spentAlpha = 0.12f;
        [SerializeField, Min(0f)] private float tickHeight = 0.012f;
        [SerializeField, Min(0f)] private float labelHeight = 0.012f;
        [SerializeField, Min(0f)] private float labelOutset = 0.008f;

        private readonly Vector3[] points = new Vector3[Points];
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
            laidHalf = -1f;
            Render(view.Readout);
        }

        private void OnDisable() => view.ReadoutChanged -= Render;

        private void LateUpdate()
        {
            if (view.FieldHalf != laidHalf || view.FieldCentre != laidCentre)
                Layout(view.FieldCentre, view.FieldHalf);
        }

        private void Layout(Vector3 centre, float half)
        {
            laidCentre = centre;
            laidHalf = half;
            transform.localPosition = centre;
            labelRoot.localPosition = new Vector3(0f, -half + labelHeight, -half - labelOutset);
            LayArm(leftArm, -1f, half);
            LayArm(rightArm, 1f, half);
            LayTick(ticks[0], PuzzleGrade.ThreeStarUse, -1f, half);
            LayTick(ticks[1], PuzzleGrade.ThreeStarUse, 1f, half);
            LayTick(ticks[2], PuzzleGrade.TwoStarUse, -1f, half);
            LayTick(ticks[3], PuzzleGrade.TwoStarUse, 1f, half);
        }

        private void LayArm(LineRenderer arm, float side, float half)
        {
            for (int i = 0; i < Points; i++)
                points[i] = FieldEdgePath.Point(i / (Points - 1f), side, half);

            arm.positionCount = Points;
            arm.SetPositions(points);
        }

        private void LayTick(LineRenderer tick, float s, float side, float half)
        {
            Vector3 foot = FieldEdgePath.Point(s, side, half);
            tick.positionCount = 2;
            tick.SetPosition(0, foot);
            tick.SetPosition(1, foot + Vector3.up * tickHeight);
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
            leftArm.colorGradient = Gradient;
            rightArm.colorGradient = Gradient;
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
