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
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;

        [Header("Scale")]
        [SerializeField] private float minHandSeparation = 0.06f;
        [SerializeField] private float scaleGain = 2.5f;
        [SerializeField] private float enterRoomScale = 0.6f;
        [SerializeField] private float exitRoomScale = 0.45f;

        private PlanetUniverseHandle firstHandle;
        private PlanetUniverseHandle secondHandle;

        private bool sessionActive;
        private bool suppressUntilRelease;
        private float startScale;
        private float startHandDistance;

        private void Start()
        {
            ghostView.Hide();
        }

        private void Update()
        {
            CollectSelection();

            if (suppressUntilRelease)
            {
                if (firstHandle == null && secondHandle == null)
                    suppressUntilRelease = false;
                return;
            }

            if (sessionActive)
                UpdateSession();
            else
                TryStartSession();
        }

        private void CollectSelection()
        {
            firstHandle = null;
            secondHandle = null;

            List<PlanetBehaviour> planets = planetsSpawner.All;
            for (int i = 0; i < planets.Count; i++)
            {
                PlanetBehaviour planet = planets[i];
                if (planet == null || !planet.IsActive)
                    continue;

                PlanetUniverseHandle handle = planet.UniverseHandle;
                if (!handle.IsSelected)
                    continue;

                if (firstHandle == null)
                    firstHandle = handle;
                else if (secondHandle == null && handle.InteractorId != firstHandle.InteractorId)
                    secondHandle = handle;
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

            if (universeView.IsMini && ghostScale >= enterRoomScale)
                Commit(false);
            else if (!universeView.IsMini && ghostScale <= exitRoomScale)
                Commit(true);
        }

        private float GetHandDistance() =>
            Vector3.Distance(firstHandle.GrabPose.position, secondHandle.GrabPose.position);

        private void UpdateGhost(float handDistance)
        {
            float ghostScale = GetGhostScale(handDistance);
            Vector3 center = (firstHandle.GrabPose.position + secondHandle.GrabPose.position) * 0.5f;
            ghostView.Show(center, universeView.ContentRadius * ghostScale);
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
            sessionActive = false;
            suppressUntilRelease = true;
            ghostView.Hide();
            universeView.SetBubbleView(toBubble, false);

            if (!universeView.IsTransitioning)
                spaceshipsSpawner.SetViewTransition(false);
        }

        private void EndSession()
        {
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
