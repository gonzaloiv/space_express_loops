using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class InFieldReadout : MonoBehaviour
    {
        [SerializeField] private SpaceShapingState state;
        [SerializeField] private ShipTravelView ship;
        [SerializeField] private Transform committedFill;
        [SerializeField] private Transform pendingFill;
        [SerializeField] private Renderer pendingRenderer;
        [SerializeField] private TMP_Text entropyLabel;
        [SerializeField] private TMP_Text windowLabel;
        [SerializeField] private TMP_Text arrivalLabel;
        [SerializeField] private Color pendingColor = new(0.23137255f, 0.36078432f, 0.9647059f, 1f);
        [SerializeField] private Color overflowColor = new(1f, 0.3019608f, 0.2509804f, 1f);
        [SerializeField] private Color idleColor = new(0.82f, 0.82f, 0.82f, 1f);
        [SerializeField] private Color insideColor = new(0.35f, 1f, 0.55f, 1f);

        private readonly StringBuilder entropyText = new();
        private readonly StringBuilder windowText = new();
        private readonly StringBuilder arrivalText = new();
        private RendererTint pendingTint;
        private PuzzleReadout readout;
        private bool hasReadout;
        private float shownClock = float.NaN;

        private void Awake() => pendingTint = new RendererTint();

        private void OnEnable()
        {
            state.ReadoutChanged += OnReadoutChanged;
            OnReadoutChanged(state.Readout);
        }

        private void OnDisable() => state.ReadoutChanged -= OnReadoutChanged;

        private void Update()
        {
            if (Mathf.Abs(ship.Clock - shownClock) >= 0.01f)
                RenderClockAndArrival();
        }

        private void OnReadoutChanged(PuzzleReadout value)
        {
            readout = value;
            hasReadout = true;
            RenderEntropy();
            RenderClockAndArrival();
        }

        private void RenderEntropy()
        {
            entropyText.Clear();
            entropyText.Append("ENTROPY  ")
                .Append(F(readout.EntropySpent));
            if (readout.EntropyPending > 0.00005f)
                entropyText.Append(" +").Append(F(readout.EntropyPending));

            entropyText.Append(" / ").Append(F(readout.EntropyBudget));
            entropyLabel.SetText(entropyText);

            float budget = Mathf.Max(readout.EntropyBudget, 0.0001f);
            float spent = Mathf.Clamp01(readout.EntropySpent / budget);
            float pending = Mathf.Min(Mathf.Clamp01(readout.EntropyPending / budget), 1f - spent);
            bool overflow = readout.EntropyBudget > 0f
                && readout.EntropySpent + readout.EntropyPending > readout.EntropyBudget;
            pendingTint.Set(pendingRenderer, overflow ? overflowColor : pendingColor);
            SetSegment(committedFill, 0f, spent);
            SetSegment(pendingFill, spent, pending);
        }

        private void RenderClockAndArrival()
        {
            float clock = ship.Clock;
            shownClock = clock;
            if (!hasReadout)
                return;

            windowText.Clear();
            windowText.Append("WINDOW  ")
                .Append(readout.Tmin.ToString("0.0", CultureInfo.InvariantCulture))
                .Append(" - ")
                .Append(readout.Tmax.ToString("0.0", CultureInfo.InvariantCulture))
                .Append(" s");
            windowLabel.SetText(windowText);
            windowLabel.color = clock >= readout.Tmin && clock <= readout.Tmax ? insideColor : idleColor;

            if (!readout.IsChecked)
            {
                if (readout.PredictedSolved)
                    arrivalLabel.SetText("PREDICTED IN WINDOW");
                else
                {
                    arrivalText.Clear();
                    arrivalText.Append("PREDICTED ").Append(Outcome());
                    arrivalLabel.SetText(arrivalText);
                }

                arrivalLabel.color = idleColor;
                return;
            }

            if (readout.IsSolved)
            {
                arrivalLabel.SetText("CHECKED IN WINDOW");
                arrivalLabel.color = insideColor;
                return;
            }

            arrivalText.Clear();
            arrivalText.Append("CHECKED ")
                .Append(Outcome())
                .Append("  ")
                .Append(PuzzleArrival.Describe(readout.ArrivalOffset));
            arrivalLabel.SetText(arrivalText);
            arrivalLabel.color = readout.IsFailed || readout.Outcome == TrajectoryOutcome.Crash
                ? overflowColor
                : idleColor;
        }

        private string Outcome() =>
            readout.Outcome == TrajectoryOutcome.Crash ? "CRASH" : "MISS";

        private static string F(float value) =>
            value.ToString("0.00", CultureInfo.InvariantCulture);

        private static void SetSegment(Transform segment, float from, float width)
        {
            Vector3 scale = segment.localScale;
            scale.x = width;
            segment.localScale = scale;

            Vector3 position = segment.localPosition;
            position.x = from + width * 0.5f - 0.5f;
            segment.localPosition = position;
        }
    }
}
