using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public enum BubbleScaleAction { None, EnterRoom, ExitRoom, Recenter }

    public class BubbleScaleSession
    {
        private readonly float scaleGain;
        private readonly float enterRoomScale;
        private readonly float exitRoomScale;
        private readonly float recenterDistance;
        private readonly float recenterHoldSecs;

        private float startScale;
        private float startHandDistance;
        private float recenterTimer;

        public BubbleScaleSession(
            float scaleGain,
            float enterRoomScale,
            float exitRoomScale,
            float recenterDistance,
            float recenterHoldSecs)
        {
            this.scaleGain = scaleGain;
            this.enterRoomScale = enterRoomScale;
            this.exitRoomScale = exitRoomScale;
            this.recenterDistance = recenterDistance;
            this.recenterHoldSecs = recenterHoldSecs;
        }

        public void Begin(float currentScale, float handDistance)
        {
            startScale = currentScale;
            startHandDistance = handDistance;
            recenterTimer = 0f;
        }

        public BubbleScaleAction Update(UniverseView view, float handDistance, float dt)
        {
            if (startHandDistance < 0.0001f)
                return BubbleScaleAction.None;

            float ghostScale = startScale
                * Mathf.Pow(Mathf.Max(handDistance / startHandDistance, 0.01f), scaleGain);
            ghostScale = Mathf.Clamp(ghostScale, view.MiniScale, 1f);

            if (view.IsMini && handDistance <= recenterDistance)
            {
                recenterTimer += dt;
                if (recenterTimer >= recenterHoldSecs)
                    return BubbleScaleAction.Recenter;
            }
            else
            {
                recenterTimer = 0f;
            }

            float feedback = view.IsMini
                ? Mathf.InverseLerp(enterRoomScale - 0.15f, enterRoomScale, ghostScale)
                : Mathf.InverseLerp(exitRoomScale + 0.15f, exitRoomScale, ghostScale);
            view.SetScaleFeedback(Mathf.Clamp01(feedback));

            if (view.IsMini && ghostScale >= enterRoomScale)
                return BubbleScaleAction.EnterRoom;

            if (!view.IsMini && ghostScale <= exitRoomScale)
                return BubbleScaleAction.ExitRoom;

            return BubbleScaleAction.None;
        }
    }
}
