using Oculus.Interaction;
using UnityEngine;
using DigitalLove.Game.Spatial;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // One grab's gesture state and math: one hand moves the frame, two hands scale the universe or
    // resize the frame depending on where they pinched. The listener owns the input plumbing.
    internal sealed class CubeGestureSession
    {
        private readonly CubeView zone;
        private readonly Grabbable grabbable;
        private readonly CubeFeedback feedback;
        private readonly CubeGestureSettings settings;

        private bool classified;
        private bool frameGrab;
        private bool moveGrab;
        private bool scaleGrab;
        private bool hadTwoHands;
        private bool moveStarted;
        private int lastCount;
        private float startHandDistance;
        private float startScale;
        private Vector3 startSize;
        private Vector3 grabOffset;
        private Vector3 grabOrigin;

        public CubeGestureSession(
            CubeView zone, Grabbable grabbable, CubeFeedback feedback, CubeGestureSettings settings)
        {
            this.zone = zone;
            this.grabbable = grabbable;
            this.feedback = feedback;
            this.settings = settings;
        }

        public bool Moved { get; private set; }

        public bool Scaled { get; private set; }

        public bool FrameGrab => frameGrab;

        // One hand holds the cube from inside its frame. The grab volume's margin is for two-hand
        // scaling only; a single pinch outside the frame neither moves nor refuses, and neither does
        // the hand left pinching after a two-hand scale.
        public bool MoveGrab => moveGrab;

        // Two hands drive the cube: resizing its frame, or scaling while the pinch centre stays inside.
        public bool TwoHandGrab => frameGrab || scaleGrab;

        public void Tick(int count)
        {
            if (!classified)
                Begin();

            TrackCount(count);
            Apply(count);
            lastCount = count;
        }

        public void End()
        {
            Moved = false;
            Scaled = false;
            zone.SetScaling(false);
            scaleGrab = false;
            moveGrab = false;
            hadTwoHands = false;
            moveStarted = false;
            classified = false;
            lastCount = 0;
        }

        private void Begin()
        {
            classified = true;
            Moved = false;
            Scaled = false;
            frameGrab = !CubeGesture.HandInsideScale(zone, grabbable);
            grabOffset = zone.Centre - HandLocal();
            grabOrigin = HandLocal();
        }

        private void TrackCount(int count)
        {
            if (count >= 2 && lastCount < 2)
            {
                startHandDistance = CubeGesture.HandDistance(grabbable);
                startScale = zone.Scale;
                startSize = zone.Size;
                bool inside = CubeGesture.PinchCentreInsideScale(zone, grabbable);
                frameGrab = zone.CanResize && !inside;
                scaleGrab = inside;
                hadTwoHands = true;
            }

            if (count == 1 && lastCount != 1)
            {
                moveGrab = !hadTwoHands && CubeGesture.HandInsideFrame(zone, grabbable);
                moveStarted = false;
                grabOrigin = HandLocal();
            }
        }

        private void Apply(int count)
        {
            if (count == 1)
                Move();
            else if (frameGrab)
                ResizeFrame();
            else if (scaleGrab)
                TryScaleUniverse();

            zone.SetScaling(count >= 2 && scaleGrab);
        }

        // A two-hand pinch only scales while its centre is in the cube: hands that drift away (down to
        // the lap, still pinched) end the scale for this grab instead of dragging the universe along.
        private void TryScaleUniverse()
        {
            if (!CubeGesture.PinchCentreInsideScale(zone, grabbable))
            {
                scaleGrab = false;
                return;
            }

            Scaled = true;
            ScaleUniverse();
        }

        private void Move()
        {
            if (!moveGrab)
                return;

            if (!zone.CanMove)
            {
                if ((HandLocal() - grabOrigin).sqrMagnitude
                    > settings.MoveRefuseDistance * settings.MoveRefuseDistance)
                    feedback.Refuse(ZoneRefusal.Locked);
                return;
            }

            if (!TryStartMove())
                return;

            Vector3 centre = zone.ConstrainMove(HandLocal() + grabOffset);
            ZonePlacement placement = zone.CanPlace(centre, zone.Size);
            if (placement.Blocked)
            {
                feedback.SetMoveRefusal(placement.Endpoint, centre, zone.Size);
                feedback.Refuse(ZoneRefusal.OutOfBounds, placement.Endpoint);
                return;
            }

            feedback.ClearMoveRefusal();
            zone.SetFrame(centre, zone.Size);
            Moved = true;
        }

        // A move starts once the hand has travelled past the dead zone, so a pinch that wobbles in place
        // does not nudge the cube; the offset is taken then, so the cube does not jump.
        private bool TryStartMove()
        {
            if (moveStarted)
                return true;

            float deadZone = settings.MoveDeadZone;
            if ((HandLocal() - grabOrigin).sqrMagnitude < deadZone * deadZone)
                return false;

            moveStarted = true;
            grabOffset = zone.Centre - HandLocal();
            return true;
        }

        private void ScaleUniverse()
        {
            if (startHandDistance < 0.0001f)
                return;

            float target = CubeGesture.ScaleTarget(
                startScale, CubeGesture.HandDistance(grabbable) / startHandDistance,
                settings.ScaleGain, feedback.MinScale, feedback.MaxScale);
            float blend = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(settings.ScaleSmoothSecs, 0.001f));
            zone.SetScale(Mathf.Lerp(zone.Scale, target, blend));
        }

        private void ResizeFrame()
        {
            if (startHandDistance < 0.0001f)
                return;

            float ratio = CubeGesture.HandDistance(grabbable) / startHandDistance;
            if (!zone.CanResize)
            {
                if (Mathf.Abs(ratio - 1f) > settings.ResizeRefuseRatio)
                    feedback.Refuse(ZoneRefusal.Locked);
                return;
            }

            Vector3 size = startSize * CubeGesture.ResizeFactor(
                ratio, settings.FrameGain, settings.FrameGrowRatio);
            if (zone.CanPlace(zone.Centre, size).Blocked)
            {
                feedback.Refuse(ZoneRefusal.OutOfBounds);
                return;
            }

            zone.SetFrame(zone.Centre, size);
        }

        private Vector3 HandLocal() => CubeGesture.HandLocal(zone, grabbable);
    }

    internal struct CubeGestureSettings
    {
        public float ScaleGain;
        public float ScaleSmoothSecs;
        public float MoveRefuseDistance;
        public float MoveDeadZone;
        public float ResizeRefuseRatio;
        public float FrameGain;
        public float FrameGrowRatio;
    }
}
