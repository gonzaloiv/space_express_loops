using System;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;
using DigitalLove.Game.Spatial;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    public class CubeGrabListener : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private CubeView zone;
        [SerializeField] private Renderer frameRenderer;
        [SerializeField] private Renderer fillRenderer;

        [Header("Palette")]
        [SerializeField] private ColorValue neutral;
        [SerializeField] private ColorValue accent;
        [SerializeField] private ColorValue success;
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

        private CubeFeedback feedback;
        private CubeGestureSession gesture;
        private bool locked;

        private readonly CubeHoverTracker hover = new();

        public event Action<ZoneRefusal> Refused
        {
            add => feedback.Refused += value;
            remove => feedback.Refused -= value;
        }

        public event Action<int> OutOfBoundsAt
        {
            add => feedback.OutOfBoundsAt += value;
            remove => feedback.OutOfBoundsAt -= value;
        }

        public event Action MoveCompleted;

        public event Action MoveEnded;

        public CubeMoveRefusal MoveRefusal => feedback.MoveRefusal;

        public Color Danger => feedback.Danger;

        // A hand is grabbing the cube (a move or a scale in progress).
        public bool IsHeld => !locked && grabbable.SelectingPointsCount > 0;

        private bool moving;

        private void Awake()
        {
            feedback = new CubeFeedback(frameRenderer, fillRenderer, BuildPalette());
            gesture = new CubeGestureSession(zone, grabbable, feedback, BuildGestureSettings());
        }

        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        public void SetLocked(bool value)
        {
            locked = value;
            if (locked)
                EndGesture();
        }

        public void ClearMoveRefusal() => feedback.ClearMoveRefusal();

        public void SetScaleEnvelope(float min, float max) => feedback.SetScaleEnvelope(min, max);

        public void SetDormant(bool value) => feedback.SetDormant(value);

        public void SetPrediction(ZoneVerdict verdict) => feedback.SetPrediction(verdict);

        public void ClearPrediction() => feedback.ClearPrediction();

        public void Refuse(ZoneRefusal reason) => feedback.Refuse(reason);

        private void OnPointerEvent(PointerEvent evt) => hover.Track(evt);

        private void Update()
        {
            if (locked)
                return;

            int count = grabbable.SelectingPointsCount;
            if (count == 0)
            {
                EndGesture();
                return;
            }

            gesture.Tick(count);
            SetMoving(count == 1 && zone.CanMove);
            ApplyFeedback(count);
        }

        private void SetMoving(bool value)
        {
            if (moving == value)
                return;

            moving = value;
            if (value)
                return;

            feedback.ClearMoveRefusal();
            MoveEnded?.Invoke();
        }

        private void EndGesture()
        {
            bool moved = gesture.Moved;
            bool scaled = gesture.Scaled;
            SetMoving(false);
            if (moved)
                MoveCompleted?.Invoke();

            if (moved && !scaled)
                zone.ResetScale();

            gesture.End();
            SnapGrabVolumeHome();
            ApplyFeedback(0);
        }

        // The Grabbable's default transformer drags and scales this grab volume with the hands; put it
        // back on the cube so the next pinch finds it.
        private void SnapGrabVolumeHome()
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }

        private void ApplyFeedback(int count) =>
            feedback.Apply(zone, count, gesture.FrameGrab, hover.Hovering, hover.World);

        private CubePalette BuildPalette() =>
            new()
            {
                Neutral = neutral.value,
                Accent = accent.value,
                Success = success.value,
                Danger = danger.value,
                FillIdleAlpha = fillIdleAlpha,
                FillActiveAlpha = fillActiveAlpha,
                RefuseSecs = refuseSecs
            };

        private CubeGestureSettings BuildGestureSettings() =>
            new()
            {
                ScaleGain = scaleGain,
                ScaleSmoothSecs = scaleSmoothSecs,
                MoveRefuseDistance = moveRefuseDistance,
                ResizeRefuseRatio = resizeRefuseRatio,
                FrameGain = frameGain,
                FrameGrowRatio = frameGrowRatio
            };
    }
}
