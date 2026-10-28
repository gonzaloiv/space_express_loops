using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipTravelView : MonoBehaviour
    {
        [SerializeField] private float travelSecs = 1.5f;
        [SerializeField] private float turnDegPerSec = 720f;
        [SerializeField] private TrailRenderer trail;

        private Coroutine flight;

        public event Action Completed;

        public bool IsFlying { get; private set; }

        public void Reset(Vector3 origin)
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
            flight = StartCoroutine(Fly(points));
        }

        public void Stop()
        {
            if (flight != null)
                StopCoroutine(flight);
            flight = null;
            IsFlying = false;
            trail.emitting = false;
            trail.Clear();
        }

        private IEnumerator Fly(List<Vector3> points)
        {
            int last = points.Count - 1;
            float travel = Mathf.Max(0.0001f, travelSecs);
            float progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime / travel;
                float index = Mathf.Clamp01(progress) * last;
                MoveTo(Sample(points, index), Heading(points, index), Time.deltaTime);
                yield return null;
            }

            transform.localPosition = points[last];
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
