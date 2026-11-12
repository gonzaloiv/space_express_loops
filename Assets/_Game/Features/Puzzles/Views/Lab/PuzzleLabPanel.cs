using System.Globalization;
using System.Text;
using DigitalLove.Game.Spatial;
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

        [SerializeField] private SpaceShapingView view;
        [SerializeField] private TMP_Text label;

        private readonly StringBuilder text = new();
        private PuzzleReadout lastReadout;
        private string message;
        private float messageUntil;
        private bool hasRendered;

        private void OnEnable()
        {
            view.ReadoutChanged += OnReadoutChanged;
            view.Refused += OnRefused;
            OnReadoutChanged(view.Readout);
        }

        private void OnDisable()
        {
            view.ReadoutChanged -= OnReadoutChanged;
            view.Refused -= OnRefused;
        }

        private void Update()
        {
            if (message == null || Time.unscaledTime < messageUntil)
                return;

            message = null;
            if (hasRendered)
                Render(lastReadout);
        }

        public void ResetPuzzle()
        {
            ClearMessage();
            view.RequestReset();
        }

        private void OnRefused(ZoneRefusal reason)
        {
            message = RefusalText(reason);
            messageUntil = Time.unscaledTime + MessageSecs;
            if (hasRendered)
                Render(lastReadout);
        }

        private void ClearMessage()
        {
            message = null;
            if (hasRendered)
                Render(lastReadout);
        }

        private void OnReadoutChanged(PuzzleReadout readout)
        {
            if (hasRendered && readout.Equals(lastReadout))
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
            if (!readout.IsChecked || !readout.IsSolved || readout.NoGrade)
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

        private static string Result(PuzzleReadout readout)
        {
            if (!readout.HasResult)
                return "SHAPING";

            if (readout.IsSolved)
                return "DELIVERED";

            if (readout.IsFailed)
                return "OUT OF ENTROPY";

            if (!readout.IsChecked)
                return readout.IsFlying ? "IN FLIGHT" : "SHAPING";

            return readout.Outcome == TrajectoryOutcome.Crash ? "CRASHED" : "MISSED";
        }

        private static string ResultColor(PuzzleReadout readout)
        {
            if (!readout.HasResult)
                return LabelColor;

            if (!readout.IsChecked)
                return readout.IsFlying ? AccentColor : FaintColor;

            if (readout.IsSolved)
                return AccentColor;

            return readout.Outcome == TrajectoryOutcome.Crash ? DangerColor : FaintColor;
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
    }
}
