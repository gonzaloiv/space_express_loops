using DigitalLove.Game.Spaceships;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseGhostController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UniverseView universeView;
        [SerializeField] private UniverseGhostView ghostView;
        [SerializeField] private TwoHandGestureInput gestureInput;
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;

        [Header("Scale")]
        [SerializeField] private float minHandSeparation = 0.06f;
        [SerializeField] private float scaleGain = 2.5f;
        [SerializeField] private float enterRoomScale = 0.6f;
        [SerializeField] private float exitRoomScale = 0.45f;

        [Header("Bubble transform")]
        [SerializeField] private float translationGain = 1f;
        [SerializeField] private float yawGain = 1f;
        [SerializeField] private float translationDeadZone = 0.01f;
        [SerializeField] private float yawDeadZone = 3f;
        [SerializeField] private float settleDistance = 0.02f;
        [SerializeField] private float settleYaw = 3f;
        [SerializeField] private float bubbleMoveLimit = 0.35f;
        [SerializeField] private float translationSharpness = 18f;
        [SerializeField] private float yawSharpness = 14f;

        [Header("Debug")]
        [SerializeField] private bool logEvents;

        private bool sessionActive;
        private bool suppressUntilRelease;
        private float startScale;
        private float startHandDistance;
        private float nextScaleLogTime;
        private BubbleMapTransformSession bubbleTransform;

        private void Awake()
        {
            bubbleTransform = new BubbleMapTransformSession(
                translationGain,
                yawGain,
                translationDeadZone,
                yawDeadZone,
                settleDistance,
                settleYaw,
                bubbleMoveLimit,
                translationSharpness,
                yawSharpness);
        }

        private void OnEnable()
        {
            universeView.TransitionCompleted += OnTransitionCompleted;
        }

        private void Start()
        {
            ghostView.Hide();
        }

        private void Update()
        {
            if (universeView.IsTransitioning)
            {
                ghostView.Hide();
                return;
            }

            if (UpdateSuppression())
                return;

            UpdateFeedback();

            if (UpdateActiveSession())
                return;

            TryStartSession();
        }

        private bool UpdateSuppression()
        {
            if (!suppressUntilRelease)
                return false;

            ghostView.Hide();
            if (!gestureInput.IsActive)
                suppressUntilRelease = false;
            return true;
        }

        private bool UpdateActiveSession()
        {
            if (!sessionActive)
                return false;

            if (!gestureInput.IsActive)
            {
                EndSession();
                return true;
            }

            UpdateSessionState();
            return true;
        }

        private void TryStartSession()
        {
            if (!gestureInput.IsActive)
                return;
            if (!universeView.HasRoomAnchor)
                return;

            float handDistance = gestureInput.HandDistance;
            if (handDistance < minHandSeparation)
                return;

            sessionActive = true;
            startScale = universeView.Scale;
            startHandDistance = handDistance;
            spaceshipsSpawner.SetViewTransition(true);
            bubbleTransform.Begin(universeView, gestureInput);
            if (logEvents)
                Debug.Log($"[UniverseGhostController] session start scale={startScale} distance={startHandDistance}");
        }

        private void UpdateSessionState()
        {
            bubbleTransform.Update(universeView, gestureInput);

            float handDistance = gestureInput.HandDistance;
            float ghostScale = GetGhostScale(handDistance);

            if (logEvents && Time.unscaledTime >= nextScaleLogTime)
            {
                nextScaleLogTime = Time.unscaledTime + 0.5f;
                Debug.Log($"[UniverseGhostController] dist={handDistance:F3} ghostScale={ghostScale:F3} mini={universeView.IsMini}");
            }

            if (universeView.IsMini && ghostScale >= enterRoomScale)
                Commit(false);
            else if (!universeView.IsMini && ghostScale <= exitRoomScale)
                Commit(true);
        }

        private void UpdateFeedback()
        {
            bool leftPinching = gestureInput.IsLeftPinching;
            bool rightPinching = gestureInput.IsRightPinching;

            if (!leftPinching && !rightPinching)
            {
                ghostView.Hide();
                return;
            }

            ghostView.Show(
                gestureInput.LeftPinchPoint,
                gestureInput.RightPinchPoint,
                leftPinching,
                rightPinching);
        }

        private float GetGhostScale(float handDistance)
        {
            if (startHandDistance < 0.0001f)
                return startScale;

            float raw = handDistance / startHandDistance;
            float scale = startScale * Mathf.Pow(Mathf.Max(raw, 0.01f), scaleGain);
            return Mathf.Clamp(scale, universeView.MiniScale, 1f);
        }

        private void Commit(bool toBubble)
        {
            if (logEvents)
                Debug.Log($"[UniverseGhostController] commit bubble={toBubble} scale={universeView.Scale}");
            sessionActive = false;
            suppressUntilRelease = true;
            bubbleTransform.Commit(universeView);
            ghostView.Hide();
            universeView.SetBubbleView(toBubble, false);
        }

        private void EndSession()
        {
            if (logEvents && sessionActive)
                Debug.Log("[UniverseGhostController] session ended");
            sessionActive = false;
            bubbleTransform.Settle(universeView);
            ghostView.Hide();
            spaceshipsSpawner.SetViewTransition(false);
        }

        private void OnTransitionCompleted(bool toBubble)
        {
            spaceshipsSpawner.SetViewTransition(false);
            spaceshipsSpawner.RefreshGrabbablesAtStation();

            if (suppressUntilRelease && !gestureInput.IsActive)
                suppressUntilRelease = false;
        }

        private void OnDisable()
        {
            universeView.TransitionCompleted -= OnTransitionCompleted;
            if (sessionActive)
                EndSession();
            ghostView.Hide();
        }
    }
}
