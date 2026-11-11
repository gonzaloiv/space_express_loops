using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // An endpoint shared by two or more routes: a neutral ring with one marker per route. In play
    // the markers carry the route colours; on the map they stay neutral, so their count still reads
    // as a station's notches.
    public class InterchangePlanetBehaviour : FieldPlanetBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private MeshRenderer markerPrototype;
        [SerializeField] private ColorValue neutralColor;
        [Tooltip("Marker centre distance from the ring's centre, in local units of the body radius.")]
        [SerializeField] private float markerRadius = 0.4675f;
        [Tooltip("Marker diameter in local units of the body radius.")]
        [SerializeField] private float markerScale = 0.12f;

        // Markers sit just above the ring's plane so the two do not z-fight.
        private const float MarkerLift = 0.015f;

        private readonly List<MeshRenderer> markers = new();
        private readonly Color[] markerColors = new Color[RouteEndpoints.MaxPips];
        private MaterialPropertyBlock tintBlock;
        private Color tint;
        private float tintAmount;
        private int markerCount;
        private bool coloured;

        public void SetPips(int count, Color[] colors)
        {
            coloured = true;
            markerCount = Mathf.Min(count, RouteEndpoints.MaxPips);
            for (int i = 0; i < markerCount; i++)
                markerColors[i] = colors[i];
            RefreshMarkers();
        }

        public void SetNotches(int count)
        {
            coloured = false;
            markerCount = Mathf.Min(count, RouteEndpoints.MaxPips);
            RefreshMarkers();
        }

        protected override void OnVisibilityChanged(bool visible)
        {
            for (int i = 0; i < markers.Count; i++)
                markers[i].gameObject.SetActive(visible && i < markerCount);
        }

        protected override void OnTintApplied(float amount, Color tint)
        {
            tintAmount = Mathf.Clamp01(amount);
            this.tint = tint;
            for (int i = 0; i < markers.Count; i++)
            {
                if (i < markerCount)
                    TintMarker(markers[i], MarkerColor(i));
            }
        }

        private void RefreshMarkers()
        {
            SetRouteColor(neutralColor.value);
            EnsureMarkers(markerCount);
            for (int i = 0; i < markers.Count; i++)
            {
                bool shown = IsVisible && i < markerCount;
                markers[i].gameObject.SetActive(shown);
                if (!shown)
                    continue;

                PlaceMarker(markers[i], i);
                TintMarker(markers[i], MarkerColor(i));
            }
        }

        private void EnsureMarkers(int count)
        {
            while (markers.Count < count)
            {
                MeshRenderer marker = Instantiate(markerPrototype, markerPrototype.transform.parent);
                marker.gameObject.name = "Pip " + (markers.Count + 1);
                marker.gameObject.SetActive(true);
                markers.Add(marker);
            }
        }

        private void PlaceMarker(MeshRenderer marker, int index)
        {
            float angle = markerCount > 0 ? 360f * index / markerCount * Mathf.Deg2Rad : 0f;
            marker.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * markerRadius, MarkerLift, Mathf.Cos(angle) * markerRadius);
            marker.transform.localScale = Vector3.one * markerScale;
            marker.transform.localRotation = Quaternion.identity;
        }

        private Color MarkerColor(int index)
        {
            if (tintAmount > 0.001f)
                return tint;

            return coloured ? markerColors[index] : neutralColor.value;
        }

        private void TintMarker(MeshRenderer marker, Color color)
        {
            tintBlock ??= new MaterialPropertyBlock();
            marker.GetPropertyBlock(tintBlock);
            tintBlock.SetColor(BaseColorId, color);
            tintBlock.SetColor(ColorId, color);
            marker.SetPropertyBlock(tintBlock);
        }
    }
}
