using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class PuzzleLabPanel : MonoBehaviour
    {
        private const string LabelColor = "#A3A3A3";
        private const string ValueColor = "#D4D4D4";
        private const string FaintColor = "#707070";
        private const string AccentColor = "#3B5CF6";
        private const string DangerColor = "#FF4D40";

        private static readonly string[] Stars = { "-", "★", "★★", "★★★" };

        [SerializeField] private SpaceShapingState state;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform committedFill;
        [SerializeField] private Transform pendingFill;
        [SerializeField] private Renderer pendingRenderer;
        [SerializeField] private Color pendingColor = new(0.23137255f, 0.36078432f, 0.9647059f, 1f);
        [SerializeField] private Color overflowColor = new(1f, 0.3019608f, 0.2509804f, 1f);

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly StringBuilder text = new();
        private MaterialPropertyBlock pendingBlock;
        private PuzzleReadout lastReadout;
        private bool hasRendered;

        private void OnEnable()
        {
            state.ReadoutChanged += OnReadoutChanged;
            OnReadoutChanged(state.Readout);
        }

        private void OnDisable() => state.ReadoutChanged -= OnReadoutChanged;

        public void ResetPuzzle() => state.ResetPuzzle();

        private void OnReadoutChanged(PuzzleReadout readout)
        {
            if (hasRendered && SameDisplay(readout, lastReadout))
                return;

            hasRendered = true;
            lastReadout = readout;
            Render(readout);
        }

        private void Render(PuzzleReadout readout)
        {
            text.Clear();
            AppendEntropyRow(readout);
            AppendWindowRow(readout);
            AppendShipRow(readout);
            AppendResultRow(readout);
            AppendGradeRow(readout);
            AppendCommandsRow(readout);
            label.SetText(text);
            SetBar(readout);
        }

        private void AppendEntropyRow(PuzzleReadout readout)
        {
            BeginRow("ENTROPY ");
            AppendTint(ValueColor, F(readout.EntropySpent));
            if (readout.EntropyPending > 0.00005f)
            {
                text.Append("<color=").Append(AccentColor).Append("> +");
                text.Append(F(readout.EntropyPending)).Append("</color>");
            }
            AppendTint(FaintColor, " / " + F(readout.EntropyBudget));
            EndRow();
        }

        private void AppendWindowRow(PuzzleReadout readout)
        {
            BeginRow("WINDOW  ");
            AppendTint(ValueColor, F(readout.Tmin) + " - " + F(readout.Tmax));
            AppendTint(FaintColor, " s");
            EndRow();
        }

        private void AppendShipRow(PuzzleReadout readout)
        {
            BeginRow("SHIP    ");
            AppendTint(ValueColor, F(readout.ShipTime));
            AppendTint(FaintColor, " s");
            EndRow();
        }

        private void AppendResultRow(PuzzleReadout readout)
        {
            BeginRow("RESULT  ");
            AppendTint(ResultColor(readout), Result(readout));
            EndRow();
        }

        private void AppendGradeRow(PuzzleReadout readout)
        {
            BeginRow("GRADE   ");
            AppendTint(readout.Grade > 0 ? ValueColor : FaintColor, Stars[readout.Grade]);
            EndRow();
        }

        private void AppendCommandsRow(PuzzleReadout readout)
        {
            BeginRow("COMMANDS");
            AppendTint(FaintColor, readout.CommandCount.ToString(CultureInfo.InvariantCulture));
            EndRow();
        }

        private void BeginRow(string name)
        {
            if (text.Length > 0)
                text.Append('\n');

            text.Append("<mspace=0.55em><color=").Append(LabelColor).Append('>')
                .Append(name).Append("</color>");
        }

        private void EndRow() => text.Append("</mspace>");

        private void AppendTint(string hex, string value) =>
            text.Append("<color=").Append(hex).Append('>').Append(value).Append("</color>");

        private void SetBar(PuzzleReadout readout)
        {
            float budget = Mathf.Max(readout.EntropyBudget, 0.0001f);
            float committed = Mathf.Clamp01(readout.EntropySpent / budget);
            float pending = Mathf.Min(Mathf.Clamp01(readout.EntropyPending / budget), 1f - committed);
            bool overflow = readout.EntropyBudget > 0f
                && readout.EntropySpent + readout.EntropyPending > readout.EntropyBudget;
            SetPendingColor(overflow ? overflowColor : pendingColor);
            SetSegment(committedFill, 0f, committed);
            SetSegment(pendingFill, committed, pending);
        }

        private void SetPendingColor(Color color)
        {
            pendingBlock ??= new MaterialPropertyBlock();
            pendingRenderer.GetPropertyBlock(pendingBlock);
            pendingBlock.SetColor(BaseColorId, color);
            pendingBlock.SetColor(ColorId, color);
            pendingRenderer.SetPropertyBlock(pendingBlock);
        }

        private static void SetSegment(Transform segment, float from, float width)
        {
            Vector3 scale = segment.localScale;
            scale.x = width;
            segment.localScale = scale;

            Vector3 position = segment.localPosition;
            position.x = from + width * 0.5f - 0.5f;
            segment.localPosition = position;
        }

        private static string Result(PuzzleReadout readout)
        {
            if (!readout.IsChecked)
                return readout.PredictedSolved ? "PREDICTED IN WINDOW" : "PREDICTED " + Outcome(readout);

            if (readout.IsSolved)
                return "CHECKED IN WINDOW";

            if (readout.IsFailed)
                return "CHECKED OUT OF ENTROPY";

            return "CHECKED " + Outcome(readout);
        }

        private static string ResultColor(PuzzleReadout readout)
        {
            if (!readout.IsChecked)
                return LabelColor;

            if (readout.IsSolved)
                return AccentColor;

            if (readout.IsFailed || readout.Outcome == TrajectoryOutcome.Crash)
                return DangerColor;

            return FaintColor;
        }

        private static string Outcome(PuzzleReadout readout) =>
            readout.Outcome == TrajectoryOutcome.Crash ? "CRASH" : "MISS";

        private static string F(float value) => value.ToString("0.0000", CultureInfo.InvariantCulture);

        private static bool SameDisplay(PuzzleReadout a, PuzzleReadout b) =>
            a.EntropySpent == b.EntropySpent
            && a.EntropyPending == b.EntropyPending
            && a.EntropyBudget == b.EntropyBudget
            && a.ShipTime == b.ShipTime
            && a.Tmin == b.Tmin
            && a.Tmax == b.Tmax
            && a.Outcome == b.Outcome
            && a.PredictedSolved == b.PredictedSolved
            && a.IsChecked == b.IsChecked
            && a.IsSolved == b.IsSolved
            && a.IsFailed == b.IsFailed
            && a.Grade == b.Grade
            && a.CommandCount == b.CommandCount;
    }
}
