using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class PuzzleLabPanel : MonoBehaviour
    {
        private static readonly string[] Stars = { "-", "★", "★★", "★★★" };

        [SerializeField] private SpaceShapingState state;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform committedFill;
        [SerializeField] private Transform pendingFill;

        private readonly StringBuilder text = new();
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
            text.Append("ENTROPY ").Append(F(readout.EntropySpent)).Append(" / ").Append(F(readout.EntropyBudget))
                .Append("\nWINDOW  ").Append(F(readout.Tmin)).Append(" - ").Append(F(readout.Tmax)).Append(" s")
                .Append("\nSHIP    ").Append(F(readout.ShipTime)).Append(" s")
                .Append("\nRESULT  ").Append(Result(readout))
                .Append("\nGRADE   ").Append(Stars[readout.Grade])
                .Append("\nCOMMANDS ").Append(readout.CommandCount);
            label.SetText(text);
            SetBar(readout);
        }

        private void SetBar(PuzzleReadout readout)
        {
            float budget = Mathf.Max(readout.EntropyBudget, 0.0001f);
            float committed = Mathf.Clamp01(readout.EntropySpent / budget);
            float pending = Mathf.Min(Mathf.Clamp01(readout.EntropyPending / budget), 1f - committed);
            SetSegment(committedFill, 0f, committed);
            SetSegment(pendingFill, committed, pending);
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
                return "PENDING";

            if (readout.IsSolved)
                return "IN WINDOW";

            if (readout.IsFailed)
                return "OUT OF ENTROPY";

            return readout.Outcome == TrajectoryOutcome.Crash ? "CRASH" : "MISS";
        }

        private static string F(float value) => value.ToString("0.0000", CultureInfo.InvariantCulture);

        private static bool SameDisplay(PuzzleReadout a, PuzzleReadout b) =>
            a.EntropySpent == b.EntropySpent
            && a.EntropyPending == b.EntropyPending
            && a.EntropyBudget == b.EntropyBudget
            && a.ShipTime == b.ShipTime
            && a.Tmin == b.Tmin
            && a.Tmax == b.Tmax
            && a.Outcome == b.Outcome
            && a.IsChecked == b.IsChecked
            && a.IsSolved == b.IsSolved
            && a.IsFailed == b.IsFailed
            && a.Grade == b.Grade
            && a.CommandCount == b.CommandCount;
    }
}
