using UnityEngine;
using UnityEngine.UI;

namespace DigitalLove.Game.SpaceShaping
{
    // Draws the entropy post on the grid's back-left vertical edge, inside the player's view of the
    // field (decided 2026-11-11; the front edge sat below the seated eye line): one line from the grid's
    // floor to its top, a notch at each star limit and a star beside each band. The owner supplies the colours;
    // this only lays out and tints authored objects.
    public class FieldEdgeMeterView : MonoBehaviour
    {
        // Enough vertices for the fixed gradient's band edges to land close to their heights.
        private const int Points = 65;

        [SerializeField] private LineRenderer post;
        [Tooltip("Top to bottom: the 3-star limit, then the 2-star limit.")]
        [SerializeField] private LineRenderer[] notches;
        [SerializeField] private RectTransform starsRoot;
        [Tooltip("Top to bottom: 3 stars, 2 stars, 1 star.")]
        [SerializeField] private RawImage[] stars;
        [SerializeField, Min(0f)] private float postOutset = 0.006f;
        [SerializeField, Min(0f)] private float notchLength = 0.012f;
        [SerializeField, Min(0f)] private float starSide = 0.022f;

        private readonly Vector3[] points = new Vector3[Points];
        private readonly GradientColorKey[] colorKeys = new GradientColorKey[4];
        private readonly GradientAlphaKey[] alphaKeys = new GradientAlphaKey[4];
        private Gradient gradient;
        private Vector3 laidCentre;
        private float laidHalf = -1f;

        private Gradient Gradient => gradient ??= new Gradient { mode = GradientMode.Fixed };

        // starHeights: one per star, top to bottom, as heights from the foot (0) to the top (1).
        public void Layout(Vector3 centre, float half, float[] starHeights)
        {
            if (centre == laidCentre && Mathf.Approximately(half, laidHalf))
                return;

            laidCentre = centre;
            laidHalf = half;
            transform.localPosition = centre;
            Vector3 foot = FieldFrame.OnMeterEdge(half, -half, postOutset);
            LayPost(foot, half * 2f);
            LayNotches(foot, half * 2f, starHeights);
            LayStars(foot, half * 2f, starHeights);
        }

        // The stops must be in ascending height order.
        public void SetBands(in MeterBands bands)
        {
            SetKey(0, bands.First);
            SetKey(1, bands.Second);
            SetKey(2, bands.Third);
            SetKey(3, bands.Fourth);
            Gradient.SetKeys(colorKeys, alphaKeys);
            post.colorGradient = Gradient;
        }

        // A noGrade job cannot award stars, so the stars and the notches that mark their limits go.
        public void SetStarsVisible(bool visible)
        {
            starsRoot.gameObject.SetActive(visible);
            for (int i = 0; i < notches.Length; i++)
                notches[i].gameObject.SetActive(visible);
        }

        public void SetStar(int index, Color color) => stars[index].color = color;

        private void SetKey(int index, MeterBand band)
        {
            colorKeys[index] = new GradientColorKey(band.Color, band.At);
            alphaKeys[index] = new GradientAlphaKey(band.Alpha, band.At);
        }

        private void LayPost(Vector3 foot, float height)
        {
            for (int i = 0; i < Points; i++)
                points[i] = foot + Vector3.up * (height * i / (Points - 1f));

            post.positionCount = Points;
            post.SetPositions(points);
        }

        private void LayNotches(Vector3 foot, float height, float[] starHeights)
        {
            for (int i = 0; i < notches.Length; i++)
            {
                Vector3 at = foot + Vector3.up * (height * starHeights[i]);
                notches[i].positionCount = 2;
                notches[i].SetPosition(0, at);
                notches[i].SetPosition(1, at + Vector3.left * notchLength);
            }
        }

        private void LayStars(Vector3 foot, float height, float[] starHeights)
        {
            starsRoot.localPosition = foot;
            float unit = Mathf.Max(starsRoot.localScale.x, 0.000001f);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].rectTransform.localPosition =
                    new Vector3(-starSide, height * starHeights[i], 0f) / unit;
            }
        }
    }

    // One stop in the post's gradient: height along the post, colour, and alpha.
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

    // The four stops the post's gradient uses.
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
