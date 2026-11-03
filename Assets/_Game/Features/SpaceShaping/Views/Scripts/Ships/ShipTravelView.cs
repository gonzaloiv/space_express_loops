using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipTravelView : MonoBehaviour
    {
        [SerializeField] private float minFlightSecs = 0.8f;
        [SerializeField] private float maxFlightSecs = 3.5f;
        [Tooltip("How much slower than simulated time the launch plays. 4 = the doc's 4x readability.")]
        [SerializeField] private float travelTimeScale = 4f;
        [SerializeField] private float launchDelaySecs = 0.09f;
        [SerializeField] private float landFadeSecs = 0.18f;
        [SerializeField] private float turnDegPerSec = 720f;
        [SerializeField] private TrailRenderer trail;

        private Coroutine flight;
        private Vector3 baseScale = Vector3.one;

        public event Action Completed;

        private void Awake() => baseScale = transform.localScale;

        // Parked at the launch point but hidden: the ship only appears once a run is flown, so the
        // initial preview shows the trajectory without giving away the ship.
        public void SoftReset(Vector3 origin)
        {
            Stop();
            transform.localPosition = origin;
            transform.localRotation = Quaternion.identity;
            gameObject.SetActive(false);
        }

        public void Travel(TrajectoryResult result, float simSecs, float delaySecs)
        {
            Stop();
            List<Vector3> points = result.Points;
            if (points.Count < 2)
                return;

            gameObject.SetActive(true);
            transform.localPosition = points[0];
            FaceHeading(points);
            trail.Clear();
            trail.emitting = false;
            flight = StartCoroutine(
                Fly(points, TravelSecs(simSecs), Mathf.Max(launchDelaySecs + delaySecs, 0f)));
        }

        public void Stop()
        {
            if (flight != null)
                StopCoroutine(flight);
            flight = null;
            trail.emitting = false;
            trail.Clear();
            transform.localScale = baseScale;
            gameObject.SetActive(false);
        }

        private float TravelSecs(float simSecs) =>
            Mathf.Clamp(
                simSecs * Mathf.Max(travelTimeScale, 0f),
                Mathf.Max(minFlightSecs, 0.0001f),
                Mathf.Max(maxFlightSecs, minFlightSecs));

        private IEnumerator Fly(List<Vector3> points, float travel, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            trail.emitting = true;
            int last = points.Count - 1;
            float progress = 0f;
            while (progress < 1f)
            {
                progress = Mathf.Min(progress + Mathf.Max(Time.deltaTime / travel, 0.0001f), 1f);
                float index = progress * last;
                MoveTo(Sample(points, index), Heading(points, index), Time.deltaTime);
                yield return null;
            }

            transform.localPosition = points[last];
            trail.emitting = false;
            yield return Land();
            gameObject.SetActive(false);
            flight = null;
            Completed?.Invoke();
        }

        private IEnumerator Land()
        {
            float secs = Mathf.Max(landFadeSecs, 0.0001f);
            float elapsed = 0f;
            while (elapsed < secs)
            {
                elapsed += Time.deltaTime;
                float remaining = 1f - Mathf.Clamp01(elapsed / secs);
                transform.localScale = baseScale * Mathf.Max(remaining, 0.001f);
                yield return null;
            }

            transform.localScale = baseScale;
        }

        private void FaceHeading(List<Vector3> points)
        {
            Vector3 heading = Heading(points, 0f);
            if (heading.sqrMagnitude < 0.000001f)
                return;

            transform.localRotation = Quaternion.LookRotation(heading, ParentUp());
        }

        private void MoveTo(Vector3 position, Vector3 forward, float deltaSecs)
        {
            transform.localPosition = position;
            if (forward.sqrMagnitude < 0.000001f)
                return;

            Quaternion target = Quaternion.LookRotation(forward, ParentUp());
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, target, turnDegPerSec * deltaSecs);
        }

        private Vector3 ParentUp() =>
            transform.parent != null
                ? transform.parent.InverseTransformDirection(Vector3.up)
                : Vector3.up;

        private static Vector3 Sample(List<Vector3> points, float index)
        {
            int last = points.Count - 1;
            int lower = Mathf.Clamp(Mathf.FloorToInt(index), 0, last);
            int upper = Mathf.Min(lower + 1, last);
            return Vector3.Lerp(points[lower], points[upper], index - lower);
        }

        private static Vector3 Heading(List<Vector3> points, float index)
        {
            int last = points.Count - 1;
            int frame = Mathf.FloorToInt(index);
            int behind = Mathf.Clamp(frame - 2, 0, last);
            int ahead = Mathf.Clamp(frame + 2, 0, last);
            return Sample(points, ahead) - Sample(points, behind);
        }
    }
}
