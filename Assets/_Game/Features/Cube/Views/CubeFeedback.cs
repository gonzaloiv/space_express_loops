using System;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // Presents a zone's frame and fill: prediction, refusal, hover, and scale colour. Owns no input.
    public sealed class CubeFeedback
    {
        private readonly Renderer frameRenderer;
        private readonly Renderer fillRenderer;
        private readonly CubePalette palette;

        private RendererTint frameTint;
        private RendererTint fillTint;

        private float minScale = 0.6f;
        private float maxScale = 1.8f;
        private bool hasPrediction;
        private ZoneVerdict prediction;
        private ZoneRefusal refusal;
        private float refusedUntil;

        public CubeFeedback(Renderer frameRenderer, Renderer fillRenderer, CubePalette palette)
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
            CubeView zone, int selectingPoints, bool frameGrab, bool hovering, Vector3 hoverWorld)
        {
            if (IsRefusing)
            {
                frameTint.Set(frameRenderer, palette.Danger);
                fillTint.Set(fillRenderer, palette.Neutral.WithAlpha(palette.FillIdleAlpha));
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
                : palette.Neutral.WithAlpha(palette.FillIdleAlpha));
        }

        private bool IsRefusing => Time.unscaledTime < refusedUntil;

        private void ApplyIdle(CubeView zone, bool hovering, Vector3 hoverWorld)
        {
            bool hoverInside = hovering && zone.ContentContains(hoverWorld);
            Color fallback = hovering ? palette.Accent : palette.Neutral;
            frameTint.Set(frameRenderer, PredictionColor(fallback));
            fillTint.Set(fillRenderer, hovering && hoverInside
                ? palette.Accent.WithAlpha(palette.FillIdleAlpha)
                : palette.Neutral.WithAlpha(palette.FillIdleAlpha));
        }

        private Color FrameActive(CubeView zone, int selectingPoints)
        {
            if (selectingPoints >= 2)
                return zone.CanResize ? palette.Accent : palette.Neutral;

            return zone.CanMove ? palette.Accent : palette.Neutral;
        }

        private Color ScaleColor(float scale) =>
            Color.Lerp(
                palette.Neutral.WithAlpha(palette.FillActiveAlpha),
                palette.Accent.WithAlpha(palette.FillActiveAlpha),
                Mathf.InverseLerp(minScale, maxScale, scale));

        private Color PredictionColor(Color none)
        {
            if (!hasPrediction)
                return none;

            if (prediction == ZoneVerdict.Solved)
                return palette.Success;

            return prediction == ZoneVerdict.Crash ? palette.Danger : palette.Neutral;
        }
    }
}
