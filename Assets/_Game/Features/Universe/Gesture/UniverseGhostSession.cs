using DigitalLove.Game.Spaceships;
using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal class UniverseGhostSession
    {
        private enum SessionMode { None, Drag, Scale }

        private readonly UniverseView universeView;
        private readonly UniverseGhostView ghostView;
        private readonly TwoHandGestureInput gestureInput;
        private readonly SpaceshipsSpawner spaceshipsSpawner;
        private readonly UniverseGhostSettings settings;
        private readonly BubbleMapTransformSession bubbleTransform;
        private readonly BubbleScaleSession bubbleScale;
        private readonly UniverseGestureGate gate;

        private bool isActive;
        private SessionMode mode;
        private bool dragLeft;

        public UniverseGhostSession(
            UniverseView universeView,
            UniverseGhostView ghostView,
            TwoHandGestureInput gestureInput,
            SpaceshipsSpawner spaceshipsSpawner,
            UniverseGhostSettings settings)
        {
            this.universeView = universeView;
            this.ghostView = ghostView;
            this.gestureInput = gestureInput;
            this.spaceshipsSpawner = spaceshipsSpawner;
            this.settings = settings;
            bubbleTransform = BubbleMapTransformSession.FromSettings(settings);
            bubbleScale = BubbleScaleSession.FromSettings(settings);
            gate = new UniverseGestureGate(settings.viewChangeCooldownSecs);
        }

        public void Start()
        {
            ghostView.Hide();
            gate.BeginCooldown();
        }

        public void Update()
        {
            BubbleViewState state = universeView.State;
            if (state.IsTransitioning)
            {
                ghostView.Hide();
                universeView.MapFeedback.Reset();
                return;
            }

            universeView.MapFeedback.SetBlocked(spaceshipsSpawner.IsAnyRouteEditing());
            if (IsRouteEditing() || gate.IsCoolingDown
                || gate.UpdateSuppression(gestureInput.State, ghostView))
            {
                universeView.MapFeedback.Reset();
                return;
            }

            UpdateGesture();
            if (!UpdateActiveSession(state))
                TryStartSession(state);
        }

        public void Stop()
        {
            if (isActive)
                EndSession();
            ghostView.Hide();
        }

        public void OnTransitionCompleted() => gate.OnTransitionCompleted(gestureInput.State);

        private bool IsRouteEditing()
        {
            if (!spaceshipsSpawner.IsAnyRouteEditing())
                return false;

            if (isActive)
                EndSession();
            ghostView.Hide();
            return true;
        }

        private void UpdateGesture()
        {
            bool isBlocked = universeView.MapFeedback.IsBlocked;
            universeView.MapFeedback.SetActive(gestureInput.State.IsAnyPinching && !isBlocked);
            if (!isActive)
                universeView.MapFeedback.SetScaleFeedback(0f);
            UniverseGhostFeedback.Refresh(ghostView, gestureInput.State);
        }

        private bool UpdateActiveSession(BubbleViewState state)
        {
            if (!isActive)
                return false;

            if (mode == SessionMode.Scale && !gestureInput.State.IsActive)
            {
                EndSession();
                return true;
            }

            if (mode == SessionMode.Drag)
            {
                if (gestureInput.State.IsActive)
                    UpgradeToScale(state);
                else if (UpdateDragHand())
                    return true;
            }

            UpdateSessionState(state);
            return true;
        }

        private bool UpdateDragHand()
        {
            bool activeHandPinching = dragLeft
                ? gestureInput.State.IsLeftPinching
                : gestureInput.State.IsRightPinching;
            if (activeHandPinching)
                return false;

            bool otherHandPinching = dragLeft
                ? gestureInput.State.IsRightPinching
                : gestureInput.State.IsLeftPinching;
            if (!otherHandPinching)
            {
                EndSession();
                gate.SuppressUntilRelease();
                return true;
            }

            dragLeft = !dragLeft;
            bubbleTransform.Rebase(gestureInput.State.DragPoint);
            return false;
        }

        private void TryStartSession(BubbleViewState state)
        {
            if (!state.HasRoomAnchor)
                return;

            if (gestureInput.State.IsActive)
            {
                float handDistance = gestureInput.State.Distance;
                if (handDistance < settings.minHandSeparation)
                    return;

                StartSession(SessionMode.Scale, handDistance, state);
                return;
            }

            if (state.IsBubble && IsDragStartValid(state))
                StartSession(SessionMode.Drag, 0f, state);
        }

        private bool IsDragStartValid(BubbleViewState state) =>
            gestureInput.State.IsSingleHandPinching
            && Vector3.Distance(gestureInput.State.DragStartPoint, state.CenterWorld)
                <= state.Metrics.BubbleRadiusWorld * 0.3f;

        private void StartSession(SessionMode sessionMode, float handDistance, BubbleViewState state)
        {
            isActive = true;
            mode = sessionMode;
            dragLeft = gestureInput.State.IsLeftPinching;
            spaceshipsSpawner.SetViewTransition(true);

            if (bubbleTransform.Begin(state, gestureInput.State.DragPoint))
                universeView.BeginBubblePreview();

            if (sessionMode == SessionMode.Scale)
                bubbleScale.Begin(state.Metrics.Scale, handDistance);
        }

        private void UpgradeToScale(BubbleViewState state)
        {
            mode = SessionMode.Scale;
            bubbleScale.Begin(state.Metrics.Scale, gestureInput.State.Distance);
        }

        private void UpdateSessionState(BubbleViewState state)
        {
            if (mode == SessionMode.Drag)
            {
                if (bubbleTransform.TryUpdate(state, gestureInput.State.DragPoint, Time.deltaTime, out Vector3 offset))
                    universeView.ApplyBubbleDrag(offset);
                return;
            }

            BubbleScaleResult result = bubbleScale.Update(
                state,
                gestureInput.State.Distance,
                Time.deltaTime);
            universeView.MapFeedback.SetScaleFeedback(result.Feedback);

            if (result.Action == BubbleScaleAction.Recenter)
                RecenterBubble();
            else if (result.Action == BubbleScaleAction.EnterRoom)
                Commit(UniverseViewMode.Room);
            else if (result.Action == BubbleScaleAction.ExitRoom)
                Commit(UniverseViewMode.Bubble);
        }

        private void RecenterBubble()
        {
            EndSession();
            universeView.ResetBubbleMap();
            universeView.MapFeedback.SetScaleFeedback(0f);
            gate.SuppressUntilRelease();
            gate.BeginCooldown(0.45f);
        }

        private void Commit(UniverseViewMode target)
        {
            isActive = false;
            mode = SessionMode.None;
            gate.SuppressUntilRelease();

            if (bubbleTransform.Commit())
                universeView.CommitBubblePreview();

            ghostView.Hide();
            universeView.SetBubbleView(target, false);
        }

        private void EndSession()
        {
            isActive = false;
            mode = SessionMode.None;

            BubbleTransformAction action = bubbleTransform.Settle(universeView.State.Metrics.ContentPosition);
            if (action == BubbleTransformAction.Cancel)
                universeView.CancelBubblePreview();
            else if (action == BubbleTransformAction.Commit)
                universeView.CommitBubblePreview();

            ghostView.Hide();
            spaceshipsSpawner.SetViewTransition(false);
        }
    }
}
