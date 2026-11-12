using System.Collections;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The line's end, authored on the trajectory prefab: a dot while the path is a prediction, a
    // sphere where the ship meets a planet (seen from every side), success-tinted on an arrival and
    // danger-tinted on a crash. A miss shows none (its heading line does).
    public class TrajectoryEndMarker : MonoBehaviour
    {
        private const float PulseSecs = 0.5f;
        private const float PulseGain = 1.4f;
        private const float HeadingEpsilon = 1e-10f;

        [SerializeField] private Renderer dot;
        [SerializeField] private Renderer arriveSphere;
        [SerializeField] private ColorValue success;
        [SerializeField] private ColorValue danger;
        [Header("Warmer, colder")]
        [Tooltip("While a pinch predicts, the dot warms as the run closes in on its destination: cold " +
                 "at this miss gap or more (field metres), fully warm when the destination catches it.")]
        [SerializeField, Min(0.001f)] private float warmReach = 0.1f;
        [Tooltip("Extra dot size when fully warm: 1.5 makes it 2.5× as big.")]
        [SerializeField, Min(0f)] private float warmGrow = 1.5f;

        private RendererTint tint;
        private Coroutine pulse;
        private Vector3 baseScale;
        private bool hasBaseScale;

        public bool IsActive => gameObject.activeSelf;

        // The prediction's dot: the route colour when the run passes far from its destination,
        // growing and turning to success as the pinch brings it in.
        public void ShowPending(Vector3 position, Vector3 heading, Color color, float missGap)
        {
            float warmth = 1f - Mathf.Clamp01(missGap / warmReach);
            Show(position, heading, dot, Color.Lerp(color, success.value, warmth));
            transform.localScale = baseScale * (1f + warmGrow * warmth);
        }

        public void ShowOutcome(Vector3 position, Vector3 heading, TrajectoryOutcome outcome)
        {
            if (outcome == TrajectoryOutcome.Arrive)
                Show(position, heading, arriveSphere, success.value);
            else if (outcome == TrajectoryOutcome.Crash)
                Show(position, heading, arriveSphere, danger.value);
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
            arriveSphere.gameObject.SetActive(shape == arriveSphere);
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
