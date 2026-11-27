using System.Collections.Generic;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // An endpoint shared by two or more routes: one neutral ring per route, stacked below the first. In
    // play the rings carry the route colours; on the map they stay neutral, so their count still reads
    // as a station's notches. Route 0 is the endpoint's own ring, which stays in NodeBody's marks.
    public class InterchangePlanetBehaviour : FieldPlanetBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private MeshRenderer routeRing;
        [SerializeField] private MeshRenderer ringPrototype;
        [SerializeField] private ColorValue neutralColor;
        [Tooltip("Vertical gap between stacked rings, in local units of the body radius.")]
        [SerializeField] private float ringGap = 0.06f;

        private readonly List<MeshRenderer> extraRings = new();
        private readonly Color[] ringColors = new Color[RouteEndpoints.MaxRings];
        private MaterialPropertyBlock tintBlock;
        private Color tint;
        private float tintAmount;
        private int ringCount;
        private bool coloured;
        private bool marksShown = true;

        public void SetRings(int count, Color[] colors)
        {
            coloured = true;
            ringCount = Mathf.Min(count, RouteEndpoints.MaxRings);
            for (int i = 0; i < ringCount; i++)
                ringColors[i] = colors[i];
            SetRouteColor(neutralColor.value);
            RefreshRings();
        }

        public void SetNotches(int count)
        {
            coloured = false;
            ringCount = Mathf.Min(count, RouteEndpoints.MaxRings);
            SetRouteColor(neutralColor.value);
            RefreshRings();
        }

        protected override void OnVisibilityChanged(bool visible) => RefreshRings();

        protected override void OnMarksShownChanged(bool shown)
        {
            marksShown = shown;
            RefreshRings();
        }

        protected override void OnTintApplied(float amount, Color tint)
        {
            tintAmount = Mathf.Clamp01(amount);
            this.tint = tint;
            RefreshRings();
        }

        private void RefreshRings()
        {
            EnsureRings(ringCount);
            for (int i = 0; i < extraRings.Count; i++)
            {
                bool shown = IsVisible && marksShown && i + 1 < ringCount;
                extraRings[i].gameObject.SetActive(shown);
                if (shown)
                    PlaceRing(extraRings[i], i + 1);
            }

            for (int i = 0; i < ringCount; i++)
                PaintRing(RingAt(i), RingColor(i));
        }

        private void EnsureRings(int count)
        {
            int extras = Mathf.Max(0, count - 1);
            while (extraRings.Count < extras)
            {
                MeshRenderer ring = Instantiate(ringPrototype, ringPrototype.transform.parent);
                ring.gameObject.name = "Ring " + (extraRings.Count + 2);
                ring.transform.localRotation = routeRing.transform.localRotation;
                ring.transform.localScale = routeRing.transform.localScale;
                ring.gameObject.SetActive(true);
                extraRings.Add(ring);
            }
        }

        private void PlaceRing(MeshRenderer ring, int index)
        {
            ring.transform.localPosition =
                routeRing.transform.localPosition + Vector3.down * (ringGap * index);
        }

        private MeshRenderer RingAt(int index) => index == 0 ? routeRing : extraRings[index - 1];

        private Color RingColor(int index)
        {
            if (tintAmount > 0.001f)
                return tint;

            return coloured ? ringColors[index] : neutralColor.value;
        }

        private void PaintRing(MeshRenderer ring, Color color)
        {
            tintBlock ??= new MaterialPropertyBlock();
            ring.GetPropertyBlock(tintBlock);
            tintBlock.SetColor(BaseColorId, color);
            tintBlock.SetColor(ColorId, color);
            ring.SetPropertyBlock(tintBlock);
        }
    }
}
