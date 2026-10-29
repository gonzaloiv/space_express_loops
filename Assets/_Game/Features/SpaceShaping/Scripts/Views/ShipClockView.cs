using TMPro;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipClockView : MonoBehaviour
    {
        [SerializeField] private ShipTravelView ship;
        [SerializeField] private SpaceShapingState state;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform face;
        [SerializeField] private float liftMetres = 0.06f;
        [SerializeField] private Color idleColor = new(0.82f, 0.82f, 0.82f, 1f);
        [SerializeField] private Color insideColor = new(0.35f, 1f, 0.55f, 1f);

        private PuzzleReadout readout;
        private bool hasReadout;
        private float shownClock = float.NaN;

        private void OnEnable()
        {
            state.ReadoutChanged += OnReadoutChanged;
            OnReadoutChanged(state.Readout);
        }

        private void OnDisable() => state.ReadoutChanged -= OnReadoutChanged;

        private void LateUpdate()
        {
            transform.position = ship.transform.position + Vector3.up * liftMetres;
            transform.rotation = Quaternion.LookRotation(transform.position - face.position, Vector3.up);
            TrackClock();
        }

        private void TrackClock()
        {
            if (Mathf.Abs(ship.Clock - shownClock) < 0.01f)
                return;

            Render();
        }

        private void OnReadoutChanged(PuzzleReadout value)
        {
            readout = value;
            hasReadout = true;
            Render();
        }

        private void Render()
        {
            float clock = ship.Clock;
            shownClock = clock;
            if (hasReadout)
                label.SetText("{0:0.0} s  [{1:0.0}–{2:0.0}]", clock, readout.Tmin, readout.Tmax);
            else
                label.SetText("{0:0.0} s", clock);

            label.color = Tint(clock);
        }

        private Color Tint(float clock) =>
            hasReadout && clock >= readout.Tmin && clock <= readout.Tmax
                ? insideColor
                : idleColor;
    }
}
