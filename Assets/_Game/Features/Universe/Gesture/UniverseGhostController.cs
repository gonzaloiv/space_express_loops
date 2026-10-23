using DigitalLove.Game.Spaceships;
using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.Universe
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
        [SerializeField] private float panLimitFactor = 1.25f;
        [SerializeField] private float settleDistance = 0.01f;
        [SerializeField] private float translationSmoothSecs = 0.08f;
        [SerializeField] private float dragStartJumpLimit = 0.12f;
        [SerializeField] private float minDragDistance = 0.03f;
        [SerializeField] private float recenterDistance = 0.05f;
        [SerializeField] private float recenterHoldSecs = 0.35f;
        [SerializeField] private float viewChangeCooldownSecs = 0.35f;

        private UniverseGhostSession session;

        private void Awake()
        {
            UniverseGhostSettings settings = new()
            {
                minHandSeparation = minHandSeparation,
                scaleGain = scaleGain,
                enterRoomScale = enterRoomScale,
                exitRoomScale = exitRoomScale,
                translationGain = translationGain,
                panLimitFactor = panLimitFactor,
                settleDistance = settleDistance,
                translationSmoothSecs = translationSmoothSecs,
                dragStartJumpLimit = dragStartJumpLimit,
                minDragDistance = minDragDistance,
                recenterDistance = recenterDistance,
                recenterHoldSecs = recenterHoldSecs,
                viewChangeCooldownSecs = viewChangeCooldownSecs
            };
            session = new UniverseGhostSession(
                universeView,
                ghostView,
                gestureInput,
                spaceshipsSpawner,
                settings);
        }

        private void OnEnable()
        {
            universeView.Mode.TransitionCompleted += OnTransitionCompleted;
        }

        private void Start() => session.Start();

        private void Update() => session.Update();

        private void OnTransitionCompleted(UniverseViewMode _) => session.OnTransitionCompleted();

        private void OnDisable()
        {
            universeView.Mode.TransitionCompleted -= OnTransitionCompleted;
            session.Stop();
        }
    }
}
