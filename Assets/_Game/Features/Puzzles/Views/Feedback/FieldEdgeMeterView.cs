using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Draws the meter's two arms and its tick rows from a frame. The owner supplies the gradient;
    // this keeps the job meter and the map's star meter on one layout.
    public class FieldEdgeMeterView : MonoBehaviour
    {
        // 96 segments per arm, so both corners (s = 0.25 and 0.75) land on a vertex.
        private const int Points = 97;

        [SerializeField] private LineRenderer leftArm;
        [SerializeField] private LineRenderer rightArm;
        [Tooltip("Left ticks first, then right. One tick per band on each arm.")]
        [SerializeField] private LineRenderer[] ticks;
        [SerializeField, Min(0f)] private float tickHeight = 0.012f;

        private readonly Vector3[] points = new Vector3[Points];
        private Vector3 laidCentre;
        private float laidHalf = -1f;
        private int laidTicks = -1;

        public void Layout(Vector3 centre, float half, int ticksPerArm)
        {
            int max = ticks.Length / 2;
            int count = Mathf.Clamp(ticksPerArm, 0, max);
            if (centre == laidCentre && Mathf.Approximately(half, laidHalf) && count == laidTicks)
                return;

            laidCentre = centre;
            laidHalf = half;
            laidTicks = count;
            transform.localPosition = centre;
            LayArm(leftArm, -1f, half);
            LayArm(rightArm, 1f, half);
            LayTicks(max, count, half);
        }

        public void SetGradient(Gradient gradient)
        {
            leftArm.colorGradient = gradient;
            rightArm.colorGradient = gradient;
        }

        private void LayArm(LineRenderer arm, float side, float half)
        {
            for (int i = 0; i < Points; i++)
                points[i] = FieldEdgePath.Point(i / (Points - 1f), side, half);

            arm.positionCount = Points;
            arm.SetPositions(points);
        }

        private void LayTicks(int max, int count, float half)
        {
            for (int i = 0; i < ticks.Length; i++)
            {
                ticks[i].positionCount = 0;
                ticks[i].enabled = false;
            }

            for (int i = 0; i < count; i++)
            {
                float s = (i + 1f) / (count + 1f);
                LayTick(ticks[i], s, -1f, half);
                LayTick(ticks[max + i], s, 1f, half);
            }
        }

        private void LayTick(LineRenderer tick, float s, float side, float half)
        {
            Vector3 foot = FieldEdgePath.Point(s, side, half);
            tick.positionCount = 2;
            tick.enabled = true;
            tick.SetPosition(0, foot);
            tick.SetPosition(1, foot + Vector3.up * tickHeight);
        }
    }
}
