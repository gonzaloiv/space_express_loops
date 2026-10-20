using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Spaceships;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseGhostController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UniverseView universeView;
        [SerializeField] private UniverseGhostView ghostView;
        [SerializeField] private PlanetsSpawner planetsSpawner;
        [SerializeField] private HubsSpawner hubsSpawner;
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;

        [Header("Scale")]
        [SerializeField] private float minHandSeparation = 0.06f;
        [SerializeField] private float scaleGain = 2.5f;
        [SerializeField] private float enterRoomScale = 0.6f;
        [SerializeField] private float exitRoomScale = 0.45f;

        [Header("Debug")]
        [SerializeField] private bool logEvents;

        private UniverseNodeHandle firstHandle;
        private UniverseNodeHandle secondHandle;

        private bool sessionActive;
        private bool suppressUntilRelease;
        private float startScale;
        private float startHandDistance;
        private int lastSelectionCount = -1;
        private float nextScaleLogTime;

        private void Start()
        {
            ghostView.Hide();
        }

        private void Update()
        {
            CollectSelection();
            LogSelectionCount();

            if (suppressUntilRelease)
            {
                ghostView.Hide();
                if (firstHandle == null && secondHandle == null)
                    suppressUntilRelease = false;
                return;
            }

            if (sessionActive)
            {
                UpdateSession();
                return;
            }

            if (firstHandle != null && secondHandle == null)
            {
                ghostView.Hide();
                return;
            }

            ghostView.Hide();
            TryStartSession();
        }

        private void CollectSelection()
        {
            firstHandle = null;
            secondHandle = null;

            List<PlanetBehaviour> planets = planetsSpawner.All;
            for (int i = 0; i < planets.Count; i++)
                TryAddNode(planets[i]);

            IReadOnlyList<HubBehaviour> hubs = hubsSpawner.All;
            for (int i = 0; i < hubs.Count; i++)
                TryAddNode(hubs[i]);
        }

        private void TryAddNode(IRouteWorldNode node)
        {
            if (!node.IsActive || !node.UniverseHandle.IsSelected)
                return;

            if (firstHandle == null)
            {
                firstHandle = node.UniverseHandle;
                return;
            }

            if (secondHandle == null && node.UniverseHandle.InteractorId != firstHandle.InteractorId)
                secondHandle = node.UniverseHandle;
        }

        private void LogSelectionCount()
        {
            if (!logEvents)
                return;

            int count = (firstHandle != null ? 1 : 0) + (secondHandle != null ? 1 : 0);
            if (count != lastSelectionCount)
            {
                lastSelectionCount = count;
                Debug.Log($"[UniverseGhostController] selected handles = {count}");
            }
        }

        private void TryStartSession()
        {
            if (firstHandle == null || secondHandle == null)
                return;
            if (universeView.IsTransitioning || !universeView.HasRoomAnchor)
                return;

            float handDistance = GetHandDistance();
            if (handDistance < minHandSeparation)
                return;

            sessionActive = true;
            startScale = universeView.Scale;
            startHandDistance = handDistance;
            spaceshipsSpawner.SetViewTransition(true);
            UpdateGhost(handDistance);
            if (logEvents)
                Debug.Log($"[UniverseGhostController] session start scale={startScale} distance={startHandDistance}");
        }

        private void UpdateSession()
        {
            if (firstHandle == null || secondHandle == null)
            {
                EndSession();
                return;
            }

            float handDistance = GetHandDistance();
            UpdateGhost(handDistance);
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

        private float GetHandDistance() =>
            Vector3.Distance(firstHandle.GrabPose.position, secondHandle.GrabPose.position);

        private void UpdateGhost(float handDistance)
        {
            Vector3 first = firstHandle.GrabPose.position;
            Vector3 second = secondHandle.GrabPose.position;
            ghostView.ShowHandLine(first, second);
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
            ghostView.Hide();
            universeView.SetBubbleView(toBubble, false);

            if (!universeView.IsTransitioning)
                spaceshipsSpawner.SetViewTransition(false);
        }

        private void EndSession()
        {
            if (logEvents && sessionActive)
                Debug.Log("[UniverseGhostController] session ended");
            sessionActive = false;
            ghostView.Hide();
            spaceshipsSpawner.SetViewTransition(false);
        }

        private void OnDisable()
        {
            EndSession();
        }
    }
}
