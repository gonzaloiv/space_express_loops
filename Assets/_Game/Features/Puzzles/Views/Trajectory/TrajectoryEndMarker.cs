using System.Collections;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The line's end, authored on the trajectory prefab: a dot while the path is a prediction, a
    // success ring on an arrival, a danger cross on a crash. A miss shows none (its heading line does).
    public class TrajectoryEndMarker : MonoBehaviour
    {
        private const float PulseSecs = 0.5f;
        private const float PulseGain = 1.4f;
        private const float HeadingEpsilon = 1e-10f;

        [SerializeField] private Renderer dot;
        [SerializeField] private Renderer arriveRing;
        [SerializeField] private Renderer crashCross;
        [SerializeField] private ColorValue success;
        [SerializeField] private ColorValue danger;

        private RendererTint tint;
        private Coroutine pulse;
        private Vector3 baseScale;
        private bool hasBaseScale;

        public bool IsActive => gameObject.activeSelf;

        public void ShowPending(Vector3 position, Vector3 heading, Color color) =>
            Show(position, heading, dot, color);

        public void ShowOutcome(Vector3 position, Vector3 heading, TrajectoryOutcome outcome)
        {
            if (outcome == TrajectoryOutcome.Arrive)
                Show(position, heading, arriveRing, success.value);
            else if (outcome == TrajectoryOutcome.Crash)
                Show(position, heading, crashCross, danger.value);
            else
                Hide();
        }

        public void Hide()
        {
            StopPulse();
            gameObject.SetActive(false);
        }

        public void Pulse()
        {
            if (!IsActive)
                return;

            StopPulse();
            pulse = StartCoroutine(PulseRoutine());
        }

        private void Show(Vector3 position, Vector3 heading, Renderer shape, Color color)
        {
            StopPulse();
            dot.gameObject.SetActive(shape == dot);
            arriveRing.gameObject.SetActive(shape == arriveRing);
            crashCross.gameObject.SetActive(shape == crashCross);
            tint.Set(shape, color);
            transform.localPosition = position;
            if (heading.sqrMagnitude > HeadingEpsilon)
                transform.localRotation = Quaternion.LookRotation(heading, Vector3.up);

            gameObject.SetActive(true);
        }

        private IEnumerator PulseRoutine()
        {
            float t = 0f;
            while (t < PulseSecs)
            {
                t += Time.deltaTime;
                float bump = 1f + Mathf.Sin(Mathf.Clamp01(t / PulseSecs) * Mathf.PI) * PulseGain;
                transform.localScale = baseScale * bump;
                yield return null;
            }

            transform.localScale = baseScale;
            pulse = null;
        }

        private void StopPulse()
        {
            if (!hasBaseScale)
            {
                baseScale = transform.localScale;
                hasBaseScale = true;
            }

            if (pulse != null)
                StopCoroutine(pulse);

            pulse = null;
            transform.localScale = baseScale;
        }
    }
}
