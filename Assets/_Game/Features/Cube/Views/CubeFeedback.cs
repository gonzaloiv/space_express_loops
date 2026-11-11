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
        private bool dormant;
        private int endpoint = -1;
        private CubeMoveRefusal moveRefusal;

        public CubeFeedback(Renderer frameRenderer, Renderer fillRenderer, CubePalette palette)
        {
            this.frameRenderer = frameRenderer;
            this.fillRenderer = fillRenderer;
            this.palette = palette;
        }

        public event Action<ZoneRefusal> Refused;

        // Fires when an out-of-bounds refusal names the endpoint it hit; the frame flash stays.
        public event Action<int> OutOfBoundsAt;

        public CubeMoveRefusal MoveRefusal => moveRefusal;

        public Color Danger => palette.Danger;

        public void SetMoveRefusal(int endpoint, Vector3 centre, Vector3 size) =>
            moveRefusal.Set(endpoint, centre, size);

        public void ClearMoveRefusal() => moveRefusal.Clear();

        public float MinScale => minScale;

        public float MaxScale => maxScale;

        public bool IsDormant => dormant;

        public void SetDormant(bool value) => dormant = value;

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

        public void Refuse(ZoneRefusal reason) => Refuse(reason, -1);

        public void Refuse(ZoneRefusal reason, int endpoint)
        {
            bool repeat = IsRefusing && refusal == reason && this.endpoint == endpoint;
            refusal = reason;
            this.endpoint = endpoint;
            refusedUntil = Time.unscaledTime + palette.RefuseSecs;
            if (repeat)
                return;

            Refused?.Invoke(reason);
            if (endpoint >= 0)
                OutOfBoundsAt?.Invoke(endpoint);
        }

        public void Apply(
            CubeView zone, int selectingPoints, bool frameGrab, bool hovering, Vector3 hoverWorld)
        {
            if (dormant)
            {
                // Danger is the map's locked colour; a dormant cube keeps it at idle alpha.
                Color dim = palette.Danger.WithAlpha(palette.FillIdleAlpha);
                frameTint.Set(frameRenderer, dim);
                fillTint.Set(fillRenderer, dim);
                return;
            }

            if (IsRefusing && !EndpointRefusal)
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

        // A move refused on an endpoint gets its own red copy; the real frame stays normal.
        private bool EndpointRefusal => refusal == ZoneRefusal.OutOfBounds && endpoint >= 0;

        private void ApplyIdle(CubeView zone, bool hovering, Vector3 hoverWorld)
        {
            bool hoverInside = hovering && zone.ScaleContains(hoverWorld);
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
