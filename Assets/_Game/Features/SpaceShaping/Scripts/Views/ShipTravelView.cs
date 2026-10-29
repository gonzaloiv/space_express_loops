using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipTravelView : MonoBehaviour
    {
        [SerializeField] private float minTravelSecs = 0.8f;
        [SerializeField] private float maxTravelSecs = 3.5f;
        [SerializeField] private float travelSecsPerShipSecond = 4f;
        [SerializeField] private float clockRateGain = 1f;
        [SerializeField] private Color slowTrailColor = new(0.35f, 0.65f, 1f, 1f);
        [SerializeField] private Color fastTrailColor = new(1f, 0.62f, 0.32f, 1f);
        [SerializeField] private float turnDegPerSec = 720f;
        [SerializeField] private TrailRenderer trail;

        private Coroutine flight;
        private float shownRate = -1f;

        public event Action Completed;

        public bool IsFlying { get; private set; }

        public float Progress { get; private set; }

        public float Clock { get; private set; }

        public void SoftReset(Vector3 origin)
        {
            Stop();
            transform.localPosition = origin;
            transform.localRotation = Quaternion.identity;
        }

        public void Travel(TrajectoryResult result)
        {
            Stop();
            List<Vector3> points = result.Points;
            if (points.Count < 2)
                return;

            transform.localPosition = points[0];
            trail.Clear();
            trail.emitting = true;
            IsFlying = true;
            flight = StartCoroutine(Fly(result, TravelSecs(result)));
        }

        public void Stop()
        {
            if (flight != null)
                StopCoroutine(flight);
            flight = null;
            IsFlying = false;
            Progress = 0f;
            Clock = 0f;
            shownRate = -1f;
            trail.emitting = false;
            trail.Clear();
        }

        private float TravelSecs(TrajectoryResult result) =>
            Mathf.Clamp(
                result.ShipTime * Mathf.Max(travelSecsPerShipSecond, 0f),
                Mathf.Max(minTravelSecs, 0.0001f),
                Mathf.Max(maxTravelSecs, minTravelSecs));

        private float SpeedAt(TrajectoryResult result, float progress)
        {
            if (clockRateGain <= 0f)
                return 1f;

            float mean = result.MeanRate;
            if (mean <= 0f)
                return 1f;

            int last = result.Points.Count - 1;
            float relative = Mathf.Clamp(result.Rate(progress * last) / mean, 0.25f, 4f);
            return Mathf.Max(0.05f, Mathf.Lerp(1f, relative, clockRateGain));
        }

        private void TintTrail(TrajectoryResult result, float index)
        {
            float mean = result.MeanRate;
            if (clockRateGain <= 0f || mean <= 0f)
                return;

            float relative = result.Rate(index) / mean;
            if (Mathf.Abs(relative - shownRate) < 0.02f)
                return;

            shownRate = relative;
            Color color = Color.Lerp(
                slowTrailColor, fastTrailColor, Mathf.InverseLerp(0.5f, 1.5f, relative));
            trail.startColor = color;
            trail.endColor = color;
        }

        private IEnumerator Fly(TrajectoryResult result, float travel)
        {
            List<Vector3> points = result.Points;
            int last = points.Count - 1;
            float progress = 0f;
            while (progress < 1f)
            {
                float step = Mathf.Max(Time.deltaTime * SpeedAt(result, progress) / travel, 0.0001f);
                progress = Mathf.Min(progress + step, 1f);
                float eased = Mathf.SmoothStep(0f, 1f, progress);
                float index = eased * last;
                Progress = eased;
                Clock = result.SampleTime(index);
                TintTrail(result, index);
                MoveTo(Sample(points, index), Heading(points, index), Time.deltaTime);
                yield return null;
            }

            transform.localPosition = points[last];
            Progress = 1f;
            Clock = result.ShipTime;
            IsFlying = false;
            flight = null;
            trail.emitting = false;
            Completed?.Invoke();
        }

        private void MoveTo(Vector3 position, Vector3 forward, float deltaSecs)
        {
            transform.localPosition = position;
            if (forward.sqrMagnitude < 0.000001f)
                return;

            Vector3 up = transform.parent != null
                ? transform.parent.InverseTransformDirection(Vector3.up)
                : Vector3.up;
            Quaternion target = Quaternion.LookRotation(forward, up);
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, target, turnDegPerSec * deltaSecs);
        }

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
