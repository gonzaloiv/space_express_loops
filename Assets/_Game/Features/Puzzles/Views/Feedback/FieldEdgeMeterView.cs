using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Draws the meter's two arms and its tick rows from a frame, and builds the band gradient.
    // The job meter and the map's star meter feed it numbers, so one layout serves both.
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
        private readonly GradientColorKey[] colorKeys = new GradientColorKey[4];
        private readonly GradientAlphaKey[] alphaKeys = new GradientAlphaKey[4];
        private Gradient gradient;
        private Vector3 laidCentre;
        private float laidHalf = -1f;
        private int laidTicks = -1;
        private float laidFirst;
        private float laidLast;

        private Gradient Gradient => gradient ??= new Gradient { mode = GradientMode.Fixed };

        // ticksPerArm counts the marks between bands; firstShare and lastShare pin where they sit.
        public void Layout(
            Vector3 centre, float half, int ticksPerArm, float firstShare, float lastShare)
        {
            int max = ticks.Length / 2;
            int count = Mathf.Clamp(ticksPerArm, 0, max);
            if (centre == laidCentre && Mathf.Approximately(half, laidHalf) && count == laidTicks
                && Mathf.Approximately(firstShare, laidFirst)
                && Mathf.Approximately(lastShare, laidLast))
                return;

            laidCentre = centre;
            laidHalf = half;
            laidTicks = count;
            laidFirst = firstShare;
            laidLast = lastShare;
            transform.localPosition = centre;
            LayArm(leftArm, -1f, half);
            LayArm(rightArm, 1f, half);
            LayTicks(max, count, half, firstShare, lastShare);
        }

        // The stops must be in ascending time order.
        public void SetBands(in MeterBands bands)
        {
            SetKey(0, bands.First);
            SetKey(1, bands.Second);
            SetKey(2, bands.Third);
            SetKey(3, bands.Fourth);
            Gradient.SetKeys(colorKeys, alphaKeys);
            leftArm.colorGradient = Gradient;
            rightArm.colorGradient = Gradient;
        }

        private void SetKey(int index, MeterBand band)
        {
            colorKeys[index] = new GradientColorKey(band.Color, band.At);
            alphaKeys[index] = new GradientAlphaKey(band.Alpha, band.At);
        }

        private void LayArm(LineRenderer arm, float side, float half)
        {
            for (int i = 0; i < Points; i++)
                points[i] = FieldEdgePath.Point(i / (Points - 1f), side, half);

            arm.positionCount = Points;
            arm.SetPositions(points);
        }

        private void LayTicks(int max, int count, float half, float first, float last)
        {
            for (int i = 0; i < ticks.Length; i++)
            {
                ticks[i].positionCount = 0;
                ticks[i].enabled = false;
            }

            for (int i = 0; i < count; i++)
            {
                float s = count > 1 ? Mathf.Lerp(first, last, i / (count - 1f)) : first;
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

    // One stop in the meter's arm gradient: position along the arm, colour, and alpha.
    public readonly struct MeterBand
    {
        public readonly float At;
        public readonly Color Color;
        public readonly float Alpha;

        public MeterBand(float at, Color color, float alpha)
        {
            At = Mathf.Clamp01(at);
            Color = color;
            Alpha = Mathf.Clamp01(alpha);
        }
    }

    // The four stops the meter's arms share.
    public readonly struct MeterBands
    {
        public readonly MeterBand First;
        public readonly MeterBand Second;
        public readonly MeterBand Third;
        public readonly MeterBand Fourth;

        public MeterBands(MeterBand first, MeterBand second, MeterBand third, MeterBand fourth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }
    }
}
