using System;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // Presents a zone's frame and fill: prediction, refusal, hover, and scale colour. Owns no input.
    public sealed class ZoneFeedback
    {
        private readonly Renderer frameRenderer;
        private readonly Renderer fillRenderer;
        private readonly ZonePalette palette;

        private RendererTint frameTint;
        private RendererTint fillTint;

        private float minScale = 0.6f;
        private float maxScale = 1.8f;
        private bool hasPrediction;
        private ZoneVerdict prediction;
        private ZoneRefusal refusal;
        private float refusedUntil;

        public ZoneFeedback(Renderer frameRenderer, Renderer fillRenderer, ZonePalette palette)
        {
            this.frameRenderer = frameRenderer;
            this.fillRenderer = fillRenderer;
            this.palette = palette;
        }

        public event Action<ZoneRefusal> Refused;

        public float MinScale => minScale;

        public float MaxScale => maxScale;

        public void SetScaleEnvelope(float min, float max)
        {
            minScale = min;
            maxScale = max;
        }

        public void SetPrediction(ZoneVerdict verdict)
        {
            hasPrediction = true;
            prediction = verdict;
        }

        public void ClearPrediction() => hasPrediction = false;

        public void Refuse(ZoneRefusal reason)
        {
            bool repeat = IsRefusing && refusal == reason;
            refusal = reason;
            refusedUntil = Time.unscaledTime + palette.RefuseSecs;
            if (!repeat)
                Refused?.Invoke(reason);
        }

        public void Apply(
            SpaceZoneView zone, int selectingPoints, bool frameGrab, bool hovering, Vector3 hoverWorld)
        {
            if (IsRefusing)
            {
                frameTint.Set(frameRenderer, palette.Danger);
                fillTint.Set(fillRenderer, Fill(palette.Neutral, palette.FillIdleAlpha));
                return;
            }

            if (selectingPoints == 0)
            {
                ApplyIdle(zone, hovering, hoverWorld);
                return;
            }

            bool scaling = selectingPoints >= 2 && !frameGrab;
            Color frameColor = scaling
                ? PredictionColor(palette.Neutral)
                : FrameActive(zone, selectingPoints);
            frameTint.Set(frameRenderer, frameColor);
            fillTint.Set(fillRenderer, scaling
                ? ScaleColor(zone.Scale)
                : Fill(palette.Neutral, palette.FillIdleAlpha));
        }

        private bool IsRefusing => Time.unscaledTime < refusedUntil;

        private void ApplyIdle(SpaceZoneView zone, bool hovering, Vector3 hoverWorld)
        {
            bool hoverInside = hovering && zone.ContentContains(hoverWorld);
            Color fallback = hovering ? palette.Accent : palette.Neutral;
            frameTint.Set(frameRenderer, PredictionColor(fallback));
            fillTint.Set(fillRenderer, hovering && hoverInside
                ? Fill(palette.Accent, palette.FillIdleAlpha)
                : Fill(palette.Neutral, palette.FillIdleAlpha));
        }

        private Color FrameActive(SpaceZoneView zone, int selectingPoints)
        {
            if (selectingPoints >= 2)
                return zone.CanResize ? palette.Accent : palette.Neutral;

            return zone.CanMove ? palette.Accent : palette.Neutral;
        }

        private Color ScaleColor(float scale) =>
            Color.Lerp(
                Fill(palette.Neutral, palette.FillActiveAlpha),
                Fill(palette.Accent, palette.FillActiveAlpha),
                Mathf.InverseLerp(minScale, maxScale, scale));

        private Color PredictionColor(Color none)
        {
            if (!hasPrediction)
                return none;

            if (prediction == ZoneVerdict.Solved)
                return palette.Success;

            return prediction == ZoneVerdict.Crash ? palette.Danger : palette.Neutral;
        }

        private static Color Fill(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
