using System;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShapingLabHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text readout;
        [SerializeField] private Color solvedColor = new(0.30f, 1f, 0.40f, 1f);
        [SerializeField] private Color overColor = new(1f, 0.28f, 0.24f, 1f);
        [SerializeField] private Color pendingColor = new(1f, 1f, 1f, 1f);

        public void Render(ShapingReadout state)
        {
            readout.color = ColorFor(state);
            readout.text = FormattableString.Invariant(
                $"{state.PuzzleName}\n{state.Outcome}\nship {state.ShipTime:0.00}s / {state.Deadline:0.00}s\nedits {state.EditCount}");
        }

        private Color ColorFor(ShapingReadout state)
        {
            if (state.Solved)
                return solvedColor;
            if (state.Outcome == TrajectoryOutcome.Arrive)
                return overColor;
            return pendingColor;
        }
    }
}
