using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ZoneGrabListener : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private SpaceZoneView zone;
        [SerializeField] private Renderer frameRenderer;
        [SerializeField] private Renderer fillRenderer;

        [Header("Frame - the region")]
        [SerializeField] private Color frameIdle = new(0.9f, 0.93f, 1f, 0.7f);
        [SerializeField] private Color frameHover = new(0.65f, 0.95f, 1f, 1f);
        [SerializeField] private Color frameMove = new(0.5f, 0.9f, 1f, 1f);
        [SerializeField] private Color frameResize = new(1f, 0.55f, 0.15f, 1f);
        [SerializeField] private float frameGain = 1.5f;
        [SerializeField] private float frameGrowRatio = 2f;

        [Header("Fill - the scaled space")]
        [SerializeField] private Color fillIdle = new(0.55f, 0.65f, 0.95f, 0.1f);
        [SerializeField] private Color fillHover = new(0.7f, 0.82f, 1f, 0.22f);
        [SerializeField] private Color fillShrink = new(0.2f, 0.6f, 1f, 0.28f);
        [SerializeField] private Color fillMagnify = new(1f, 0.45f, 0.2f, 0.28f);
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private float scaleGain = 2f;
        [SerializeField] private float scaleSmoothSecs = 0.08f;
        [SerializeField] private float moveRefuseDistance = 0.02f;
        [SerializeField] private float resizeRefuseRatio = 0.03f;

        [Header("Prediction")]
        [SerializeField] private Color frameSolved = new(0.35f, 1f, 0.55f, 1f);
        [SerializeField] private Color frameMiss = new(0.75f, 0.85f, 1f, 1f);
        [SerializeField] private Color frameCrash = new(1f, 0.35f, 0.3f, 1f);
        [SerializeField] private Color frameRefused = new(1f, 0.25f, 0.2f, 1f);
        [SerializeField] private float refuseSecs = 0.18f;

        private RendererTint frameTint;
        private RendererTint fillTint;

        private bool hasPrediction;
        private bool predictedSolved;
        private TrajectoryOutcome predictedOutcome;
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

        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        public void SetPrediction(bool solved, TrajectoryOutcome outcome)
        {
            hasPrediction = true;
            predictedSolved = solved;
            predictedOutcome = outcome;
        }

        public void Refuse() => refusedUntil = Time.unscaledTime + refuseSecs;

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
                    Refuse();
                return;
            }

            zone.SetFrame(HandLocal() + grabOffset, zone.Size);
        }

        private void ScaleUniverse()
        {
            if (startHandDistance < 0.0001f)
                return;

            float target = ZoneGesture.ScaleTarget(
                startScale, ZoneGesture.HandDistance(grabbable) / startHandDistance, scaleGain,
                settings.minScaleFactor, settings.maxScaleFactor);
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
                    Refuse();
                return;
            }

            zone.SetFrame(zone.Centre, startSize * ZoneGesture.ResizeFactor(ratio, frameGain, frameGrowRatio));
        }

        private void ApplyFeedback()
        {
            if (IsRefusing)
            {
                frameTint.Set(frameRenderer, frameRefused);
                fillTint.Set(fillRenderer, fillIdle);
                return;
            }

            if (grabbable.SelectingPointsCount == 0)
            {
                ApplyIdleFeedback();
                return;
            }

            bool scaling = grabbable.SelectingPointsCount >= 2 && !frameGrab;
            Color frameColor = scaling ? PredictionColor(frameMiss, frameIdle) : FrameActiveColor();
            frameTint.Set(frameRenderer, frameColor);
            fillTint.Set(fillRenderer, scaling ? ScaleColor() : fillIdle);
        }

        private Color PredictionColor(Color miss, Color none)
        {
            if (!hasPrediction)
                return none;

            if (predictedSolved)
                return frameSolved;

            return predictedOutcome == TrajectoryOutcome.Crash ? frameCrash : miss;
        }

        private void ApplyIdleFeedback()
        {
            bool hoverInside = hovering && zone.ContentContains(hoverWorld);
            Color fallback = hovering && !hoverInside ? frameHover : frameIdle;
            frameTint.Set(frameRenderer, PredictionColor(fallback, fallback));
            fillTint.Set(fillRenderer, hovering && hoverInside ? fillHover : fillIdle);
        }

        private Color FrameActiveColor()
        {
            if (grabbable.SelectingPointsCount >= 2)
                return zone.CanResize ? frameResize : frameIdle;

            return zone.CanMove ? frameMove : frameIdle;
        }

        private Color ScaleColor() =>
            Color.Lerp(fillShrink, fillMagnify, Mathf.InverseLerp(
                settings.minScaleFactor, settings.maxScaleFactor, zone.Scale));

        private Vector3 HandLocal() => ZoneGesture.HandLocal(zone, grabbable);
    }
}
