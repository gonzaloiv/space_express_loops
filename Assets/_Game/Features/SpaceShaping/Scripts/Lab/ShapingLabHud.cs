using System;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShapingLabHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text readout;
        [SerializeField] private Color solvedColor = new(0.30f, 1f, 0.40f, 1f);
        [SerializeField] private Color failColor = new(1f, 0.28f, 0.24f, 1f);
        [SerializeField] private Color pendingColor = new(1f, 1f, 1f, 1f);

        public void Render(ShapingReadout state)
        {
            readout.color = ColorFor(state);
            readout.text = FormattableString.Invariant(
                $"{state.PuzzleName}\n{OutcomeText(state)}\nscale {state.Scale:0.00}\nship {state.ShipTime:0.00}s in [{state.Tmin:0.00}–{state.Tmax:0.00}s]\nentropy {state.EntropyLeft:0.00} ({GradeText(state)})\nzones {state.Zones}");
        }

        private Color ColorFor(ShapingReadout state)
        {
            if (!state.Verified)
                return pendingColor;

            return state.Solved ? solvedColor : failColor;
        }

        private static string OutcomeText(ShapingReadout state) =>
            state.Verified ? state.Outcome.ToString() : "—";

        private static string GradeText(ShapingReadout state) =>
            state.Verified ? state.Stars + "*" : "—";
    }
}
