using System.Globalization;
using System.Text;
using DigitalLove.Game.SpaceShaping.Cube;
using DigitalLove.Global;
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
        private const float HeadlinePercent = 160f;
        private const float MessageSecs = 2f;

        [SerializeField] private SpaceShapingController state;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform committedFill;
        [SerializeField] private Transform pendingFill;
        [SerializeField] private Renderer pendingRenderer;
        [SerializeField] private ColorValue accent;
        [SerializeField] private ColorValue danger;

        private readonly StringBuilder text = new();
        private RendererTint pendingTint;
        private PuzzleReadout lastReadout;
        private string message;
        private float messageUntil;
        private bool hasRendered;

        private void OnEnable()
        {
            state.ReadoutChanged += OnReadoutChanged;
            state.Refused += OnRefused;
            OnReadoutChanged(state.Readout);
        }

        private void OnDisable()
        {
            state.ReadoutChanged -= OnReadoutChanged;
            state.Refused -= OnRefused;
        }

        private void Update()
        {
            if (message == null || Time.unscaledTime < messageUntil)
                return;

            message = null;
            if (hasRendered)
                Render(lastReadout);
        }

        public void ResetPuzzle() => state.ResetPuzzle();

        public void NextPuzzle() => state.NextPuzzle();

        private void OnRefused(ZoneRefusal reason)
        {
            message = RefusalText(reason);
            messageUntil = Time.unscaledTime + MessageSecs;
            if (hasRendered)
                Render(lastReadout);
        }

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
            AppendHeadline(readout);
            AppendEntropyRow(readout);
            AppendGradeRow(readout);
            AppendMessageRow();
            label.SetText(text);
            SetBar(readout);
        }

        private void AppendHeadline(PuzzleReadout readout)
        {
            text.Append("<size=")
                .Append(HeadlinePercent.ToString(CultureInfo.InvariantCulture)).Append("%>");
            AppendTint(ResultColor(readout), Result(readout));
            text.Append("</size>");
        }

        private void AppendEntropyRow(PuzzleReadout readout)
        {
            BeginRow("ENTROPY");
            AppendTint(ValueColor, F(readout.EntropyBudget - readout.EntropySpent));
            if (readout.EntropyPending > 0.00005f)
            {
                text.Append("<color=").Append(AccentColor).Append("> -");
                text.Append(F(readout.EntropyPending)).Append("</color>");
            }
            AppendTint(FaintColor, " / " + F(readout.EntropyBudget));
            EndRow();
        }

        private void AppendGradeRow(PuzzleReadout readout)
        {
            if (!readout.IsChecked || !readout.IsSolved)
                return;

            BeginRow("GRADE  ");
            AppendTint(GradeColor(readout.Grade), PuzzleReward.Describe(readout.Grade));
            EndRow();
        }

        private void AppendMessageRow()
        {
            if (message == null)
                return;

            text.Append('\n');
            AppendTint(DangerColor, message);
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
            float spent = Mathf.Clamp01(readout.EntropySpent / budget);
            float pending = Mathf.Min(Mathf.Clamp01(readout.EntropyPending / budget), 1f - spent);
            float remaining = 1f - spent;
            bool overflow = readout.EntropyBudget > 0f
                && readout.EntropySpent + readout.EntropyPending > readout.EntropyBudget;
            SetPendingColor(overflow ? danger.value : accent.value);
            SetSegment(committedFill, 0f, remaining);
            SetSegment(pendingFill, remaining - pending, pending);
        }

        private void SetPendingColor(Color color) => pendingTint.Set(pendingRenderer, color);

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
            if (!readout.HasResult)
                return "SHAPING";

            if (readout.IsSolved)
                return "DELIVERED";

            if (readout.IsFailed)
                return "OUT OF ENTROPY";

            bool crash = readout.Outcome == TrajectoryOutcome.Crash;
            if (!readout.IsChecked)
                return crash ? "WILL CRASH" : "WILL MISS";

            return crash ? "CRASHED" : "MISSED";
        }

        private static string ResultColor(PuzzleReadout readout)
        {
            if (!readout.HasResult)
                return LabelColor;

            if (readout.Outcome == TrajectoryOutcome.Crash || readout.IsFailed)
                return DangerColor;

            return readout.PredictedSolved ? AccentColor : FaintColor;
        }

        private static string RefusalText(ZoneRefusal reason)
        {
            switch (reason)
            {
                case ZoneRefusal.Locked:
                    return "ZONE IS FIXED";
                case ZoneRefusal.OutOfBounds:
                    return "KEEP CLEAR OF THE PLANETS";
                case ZoneRefusal.OutOfEntropy:
                    return "OUT OF ENTROPY - RESET";
                default:
                    return string.Empty;
            }
        }

        private static string GradeColor(int grade) =>
            grade >= 3 ? AccentColor : grade > 0 ? ValueColor : FaintColor;

        private static string F(float value) => value.ToString("0.00", CultureInfo.InvariantCulture);

        private static bool SameDisplay(PuzzleReadout a, PuzzleReadout b) =>
            a.EntropySpent == b.EntropySpent
            && a.EntropyPending == b.EntropyPending
            && a.EntropyBudget == b.EntropyBudget
            && a.Outcome == b.Outcome
            && a.HasResult == b.HasResult
            && a.PredictedSolved == b.PredictedSolved
            && a.IsChecked == b.IsChecked
            && a.IsSolved == b.IsSolved
            && a.IsFailed == b.IsFailed
            && a.Grade == b.Grade;
    }
}
