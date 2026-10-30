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

        private ZoneFeedback feedback;

        private bool classified;
        private bool frameGrab;
        private bool moved;
        private bool scaled;
        private int lastCount;
        private float startHandDistance;
        private float startScale;
        private Vector3 startSize;
        private Vector3 grabOffset;
        private Vector3 grabOrigin;

        private bool hovering;
        private Vector3 hoverWorld;

        public event Action<ZoneRefusal> Refused
        {
            add => feedback.Refused += value;
            remove => feedback.Refused -= value;
        }

        private void Awake() => feedback = new ZoneFeedback(frameRenderer, fillRenderer, BuildPalette());

        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        public void SetScaleEnvelope(float min, float max) => feedback.SetScaleEnvelope(min, max);

        public void SetPrediction(ZoneVerdict verdict) => feedback.SetPrediction(verdict);

        public void ClearPrediction() => feedback.ClearPrediction();

        public void Refuse(ZoneRefusal reason) => feedback.Refuse(reason);

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
            ApplyFeedback(count);
            lastCount = count;
        }

        private void EndGesture()
        {
            if (moved && !scaled)
                zone.ResetScale();

            moved = false;
            scaled = false;
            zone.SetScaling(false);
            classified = false;
            lastCount = 0;
            ApplyFeedback(0);
        }

        private void ApplyFeedback(int count) =>
            feedback.Apply(zone, count, frameGrab, hovering, hoverWorld);

        private void TrackCount(int count)
        {
            if (count >= 2 && lastCount < 2)
            {
                startHandDistance = ZoneGesture.HandDistance(grabbable);
                startScale = zone.Scale;
                startSize = zone.Size;
                frameGrab = !ZoneGesture.AllHandsInsideScale(zone, grabbable);
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
            {
                scaled = true;
                ScaleUniverse();
            }

            zone.SetScaling(count >= 2 && !frameGrab);
        }

        private void BeginGesture()
        {
            classified = true;
            moved = false;
            scaled = false;
            frameGrab = !ZoneGesture.HandInsideScale(zone, grabbable);
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
            moved = true;
        }

        private void ScaleUniverse()
        {
            if (startHandDistance < 0.0001f)
                return;

            float target = ZoneGesture.ScaleTarget(
                startScale, ZoneGesture.HandDistance(grabbable) / startHandDistance, scaleGain,
                feedback.MinScale, feedback.MaxScale);
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

        private ZonePalette BuildPalette() =>
            new()
            {
                Neutral = neutral.value,
                NeutralHover = neutralHover.value,
                Slow = slow.value,
                Fast = fast.value,
                Success = success.value,
                Miss = miss.value,
                Danger = danger.value,
                FillIdleAlpha = fillIdleAlpha,
                FillActiveAlpha = fillActiveAlpha,
                RefuseSecs = refuseSecs
            };

        private Vector3 HandLocal() => ZoneGesture.HandLocal(zone, grabbable);
    }
}
