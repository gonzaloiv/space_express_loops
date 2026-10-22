using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal enum BubbleScaleAction { None, EnterRoom, ExitRoom, Recenter }

    internal readonly struct BubbleScaleResult
    {
        public readonly BubbleScaleAction Action;
        public readonly float Feedback;

        public BubbleScaleResult(BubbleScaleAction action, float feedback)
        {
            Action = action;
            Feedback = feedback;
        }
    }

    internal class BubbleScaleSession
    {
        private const float FeedbackBand = 0.15f;

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

        public static BubbleScaleSession FromSettings(UniverseGhostSettings settings) =>
            new(settings.scaleGain,
                settings.enterRoomScale,
                settings.exitRoomScale,
                settings.recenterDistance,
                settings.recenterHoldSecs);

        public void Begin(float currentScale, float handDistance)
        {
            startScale = currentScale;
            startHandDistance = handDistance;
            recenterTimer = 0f;
        }

        public BubbleScaleResult Update(BubbleViewState state, float handDistance, float dt)
        {
            if (startHandDistance < 0.0001f)
                return new BubbleScaleResult(BubbleScaleAction.None, 0f);

            float ghostScale = CalculateGhostScale(state, handDistance);
            if (IsRecenterRequested(state, handDistance, dt))
                return new BubbleScaleResult(BubbleScaleAction.Recenter, 0f);

            float feedback = CalculateFeedback(state, ghostScale);
            return new BubbleScaleResult(GetRoomAction(state, ghostScale), Mathf.Clamp01(feedback));
        }

        private float CalculateGhostScale(BubbleViewState state, float handDistance)
        {
            float ghostScale = startScale
                * Mathf.Pow(Mathf.Max(handDistance / startHandDistance, 0.01f), scaleGain);
            return Mathf.Clamp(ghostScale, state.Metrics.MiniScale, 1f);
        }

        private bool IsRecenterRequested(BubbleViewState state, float handDistance, float dt)
        {
            if (!state.IsBubble || handDistance > recenterDistance)
            {
                recenterTimer = 0f;
                return false;
            }

            recenterTimer += dt;
            return recenterTimer >= recenterHoldSecs;
        }

        private float CalculateFeedback(BubbleViewState state, float ghostScale)
        {
            return state.IsBubble
                ? Mathf.InverseLerp(enterRoomScale - FeedbackBand, enterRoomScale, ghostScale)
                : Mathf.InverseLerp(exitRoomScale + FeedbackBand, exitRoomScale, ghostScale);
        }

        private BubbleScaleAction GetRoomAction(BubbleViewState state, float ghostScale)
        {
            if (state.IsBubble && ghostScale >= enterRoomScale)
                return BubbleScaleAction.EnterRoom;

            if (!state.IsBubble && ghostScale <= exitRoomScale)
                return BubbleScaleAction.ExitRoom;

            return BubbleScaleAction.None;
        }
    }
}
