using System;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public class ZoneGrabListener : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private SpaceZoneView zone;
        [SerializeField] private Renderer frameRenderer;
        [SerializeField] private Renderer fillRenderer;

        [Header("Palette")]
        [SerializeField] private ColorValue neutral;
        [SerializeField] private ColorValue neutralHover;
        [SerializeField] private ColorValue slow;
        [SerializeField] private ColorValue fast;
        [SerializeField] private ColorValue success;
        [SerializeField] private ColorValue miss;
        [SerializeField] private ColorValue danger;

        [Header("Frame - the region")]
        [SerializeField] private float frameGain = 1.5f;
        [SerializeField] private float frameGrowRatio = 2f;

        [Header("Fill - the scaled space")]
        [SerializeField] private float fillIdleAlpha = 0.1f;
        [SerializeField] private float fillActiveAlpha = 0.45f;

        [Header("Scale")]
        [SerializeField] private float scaleGain = 0.9f;
        [SerializeField] private float scaleSmoothSecs = 0.2f;
        [SerializeField] private float moveRefuseDistance = 0.02f;
        [SerializeField] private float resizeRefuseRatio = 0.03f;

        [Header("Refuse")]
        [SerializeField] private float refuseSecs = 0.18f;

        private RendererTint frameTint;
        private RendererTint fillTint;

        private float minScaleFactor = 0.6f;
        private float maxScaleFactor = 1.8f;

        private bool hasPrediction;
        private ZoneVerdict predictedVerdict;
        private ZoneRefusal refusal;
        private float refusedUntil;

        private bool classified;
        private bool frameGrab;
        private int lastCount;
        private float startHandDistance;
        private float startScale;
        private Vector3 startSize;
        private Vector3 grabOffset;
        private Vector3 grabOrigin;

        private bool hovering;
        private Vector3 hoverWorld;

        public event Action<ZoneRefusal> Refused;

        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        public void SetScaleEnvelope(float min, float max)
        {
            minScaleFactor = min;
            maxScaleFactor = max;
        }

        public void SetPrediction(ZoneVerdict verdict)
        {
            hasPrediction = true;
            predictedVerdict = verdict;
        }

        public void ClearPrediction() => hasPrediction = false;

        public void Refuse(ZoneRefusal reason)
        {
            bool repeat = IsRefusing && refusal == reason;
            refusal = reason;
            refusedUntil = Time.unscaledTime + refuseSecs;
            if (!repeat)
                Refused?.Invoke(reason);
        }

        private bool IsRefusing => Time.unscaledTime < refusedUntil;

        private void OnPointerEvent(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Hover:
                case PointerEventType.Move:
                    hoverWorld = evt.Pose.position;
                    hovering = true;
                    break;
                case PointerEventType.Unhover:
                case PointerEventType.Cancel:
                    hovering = false;
                    break;
            }
        }

        private void Update()
        {
            int count = grabbable.SelectingPointsCount;
            if (count == 0)
            {
                EndGesture();
                return;
            }

            if (!classified)
                BeginGesture();

            TrackCount(count);
            ApplyGesture(count);
            ApplyFeedback();
            lastCount = count;
        }

        private void EndGesture()
        {
            zone.SetScaling(false);
            classified = false;
            lastCount = 0;
            ApplyFeedback();
        }

        private void TrackCount(int count)
        {
            if (count >= 2 && lastCount < 2)
            {
                startHandDistance = ZoneGesture.HandDistance(grabbable);
                startScale = zone.Scale;
                startSize = zone.Size;
                frameGrab = !ZoneGesture.AllHandsInside(zone, grabbable);
            }

            if (count == 1 && lastCount != 1)
            {
                grabOffset = zone.Centre - HandLocal();
                grabOrigin = HandLocal();
            }
        }

        private void ApplyGesture(int count)
        {
            if (count == 1)
                Move();
            else if (frameGrab)
                ResizeFrame();
            else
                ScaleUniverse();

            zone.SetScaling(count >= 2 && !frameGrab);
        }

        private void BeginGesture()
        {
            classified = true;
            frameGrab = !ZoneGesture.HandInsideContent(zone, grabbable);
            grabOffset = zone.Centre - HandLocal();
            grabOrigin = HandLocal();
        }

        private void Move()
        {
            if (!zone.CanMove)
            {
                if ((HandLocal() - grabOrigin).sqrMagnitude > moveRefuseDistance * moveRefuseDistance)
                    Refuse(ZoneRefusal.Locked);
                return;
            }

            Vector3 centre = HandLocal() + grabOffset;
            if (!zone.CanPlace(centre, zone.Size))
            {
                Refuse(ZoneRefusal.OutOfBounds);
                return;
            }

            zone.SetFrame(centre, zone.Size);
        }

        private void ScaleUniverse()
        {
            if (startHandDistance < 0.0001f)
                return;

            float target = ZoneGesture.ScaleTarget(
                startScale, ZoneGesture.HandDistance(grabbable) / startHandDistance, scaleGain,
                minScaleFactor, maxScaleFactor);
            float blend = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(scaleSmoothSecs, 0.001f));
            zone.SetScale(Mathf.Lerp(zone.Scale, target, blend));
        }

        private void ResizeFrame()
        {
            if (startHandDistance < 0.0001f)
                return;

            float ratio = ZoneGesture.HandDistance(grabbable) / startHandDistance;
            if (!zone.CanResize)
            {
                if (Mathf.Abs(ratio - 1f) > resizeRefuseRatio)
                    Refuse(ZoneRefusal.Locked);
                return;
            }

            Vector3 size = startSize * ZoneGesture.ResizeFactor(ratio, frameGain, frameGrowRatio);
            if (!zone.CanPlace(zone.Centre, size))
            {
                Refuse(ZoneRefusal.OutOfBounds);
                return;
            }

            zone.SetFrame(zone.Centre, size);
        }

        private void ApplyFeedback()
        {
            if (IsRefusing)
            {
                frameTint.Set(frameRenderer, danger.value);
                fillTint.Set(fillRenderer, Fill(neutral, fillIdleAlpha));
                return;
            }

            if (grabbable.SelectingPointsCount == 0)
            {
                ApplyIdleFeedback();
                return;
            }

            bool scaling = grabbable.SelectingPointsCount >= 2 && !frameGrab;
            Color frameColor = scaling ? PredictionColor(miss.value, neutral.value) : FrameActiveColor();
            frameTint.Set(frameRenderer, frameColor);
            fillTint.Set(fillRenderer, scaling ? ScaleColor() : Fill(neutral, fillIdleAlpha));
        }

        private Color PredictionColor(Color missColor, Color none)
        {
            if (!hasPrediction)
                return none;

            if (predictedVerdict == ZoneVerdict.Solved)
                return success.value;

            return predictedVerdict == ZoneVerdict.Crash ? danger.value : missColor;
        }

        private void ApplyIdleFeedback()
        {
            bool hoverInside = hovering && zone.ContentContains(hoverWorld);
            Color fallback = hovering && !hoverInside ? neutralHover.value : neutral.value;
            frameTint.Set(frameRenderer, PredictionColor(fallback, fallback));
            fillTint.Set(fillRenderer, hovering && hoverInside
                ? Fill(neutralHover, fillIdleAlpha)
                : Fill(neutral, fillIdleAlpha));
        }

        private Color FrameActiveColor()
        {
            if (grabbable.SelectingPointsCount >= 2)
                return zone.CanResize ? fast.value : neutral.value;

            return zone.CanMove ? neutralHover.value : neutral.value;
        }

        private Color ScaleColor() =>
            Color.Lerp(Fill(slow, fillActiveAlpha), Fill(fast, fillActiveAlpha), Mathf.InverseLerp(
                minScaleFactor, maxScaleFactor, zone.Scale));

        private static Color Fill(ColorValue color, float alpha)
        {
            Color value = color.value;
            value.a = alpha;
            return value;
        }

        private Vector3 HandLocal() => ZoneGesture.HandLocal(zone, grabbable);
    }
}
