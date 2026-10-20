using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Spaceships;
using Oculus.Interaction;
using Oculus.Interaction.Input;
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
        [SerializeField] private HandRef leftHand;
        [SerializeField] private HandRef rightHand;
        [SerializeField] private TouchHandGrabInteractor leftShipGrab;
        [SerializeField] private TouchHandGrabInteractor rightShipGrab;

        [Header("Hands")]
        [SerializeField] private float pinchThreshold = 0.7f;
        [SerializeField] private float grabPadding = 0.035f;
        [SerializeField] private float minHandSeparation = 0.06f;

        [Header("Scale")]
        [SerializeField] private float scaleGain = 2.5f;
        [SerializeField] private float enterRoomScale = 0.6f;
        [SerializeField] private float exitRoomScale = 0.45f;

        private readonly HandState left = new();
        private readonly HandState right = new();

        private bool sessionActive;
        private bool suppressUntilRelease;
        private float startScale;
        private float startHandDistance;

        private void Start()
        {
            left.hand = leftHand;
            left.shipGrab = leftShipGrab;
            right.hand = rightHand;
            right.shipGrab = rightShipGrab;

            ghostView.Hide();
        }

        private void Update()
        {
            UpdateHand(left, right);
            UpdateHand(right, left);

            if (suppressUntilRelease)
            {
                if (!left.isPinching && !right.isPinching)
                    suppressUntilRelease = false;
                return;
            }

            if (sessionActive)
                UpdateSession();
            else
                TryStartSession();
        }

        private void TryStartSession()
        {
            if (!left.isPinching || !right.isPinching || left.planet == right.planet)
                return;
            if (universeView.IsTransitioning || !universeView.HasRoomAnchor)
                return;

            float handDistance = Vector3.Distance(left.pinchPoint, right.pinchPoint);
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
            if (!left.isPinching || !right.isPinching || left.planet == right.planet)
            {
                EndSession();
                return;
            }

            float handDistance = Vector3.Distance(left.pinchPoint, right.pinchPoint);
            UpdateGhost(handDistance);
            float ghostScale = GetGhostScale(handDistance);

            if (universeView.IsMini && ghostScale >= enterRoomScale)
                Commit(false);
            else if (!universeView.IsMini && ghostScale <= exitRoomScale)
                Commit(true);
        }

        private void UpdateGhost(float handDistance)
        {
            float ghostScale = GetGhostScale(handDistance);
            Vector3 center = (left.pinchPoint + right.pinchPoint) * 0.5f;
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

        private void UpdateHand(HandState state, HandState other)
        {
            state.Clear();

            HandRef hand = state.hand;
            if (!hand.IsConnected || !hand.IsTrackedDataValid)
                return;
            if (state.shipGrab.State == InteractorState.Select)
                return;
            if (hand.GetFingerPinchStrength(HandFinger.Index) < pinchThreshold)
                return;
            if (!hand.GetJointPose(HandJointId.HandThumbTip, out Pose thumb)
                || !hand.GetJointPose(HandJointId.HandIndexTip, out Pose index))
                return;

            Vector3 pinchPoint = (thumb.position + index.position) * 0.5f;
            PlanetBehaviour planet = FindPlanet(pinchPoint, other.planet);
            if (planet == null)
                return;

            state.planet = planet;
            state.pinchPoint = pinchPoint;
            state.isPinching = true;
        }

        private PlanetBehaviour FindPlanet(Vector3 point, PlanetBehaviour exclude)
        {
            PlanetBehaviour best = null;
            float bestDistance = float.MaxValue;
            List<PlanetBehaviour> planets = planetsSpawner.All;
            for (int i = 0; i < planets.Count; i++)
            {
                PlanetBehaviour planet = planets[i];
                if (planet == null || !planet.IsActive || planet == exclude)
                    continue;

                float radius = planet.NodeBody.Radius * universeView.Scale + grabPadding;
                float distance = (planet.Position - point).sqrMagnitude;
                if (distance > radius * radius || distance >= bestDistance)
                    continue;

                best = planet;
                bestDistance = distance;
            }

            return best;
        }

        private void OnDisable()
        {
            EndSession();
        }

        private class HandState
        {
            public HandRef hand;
            public TouchHandGrabInteractor shipGrab;
            public PlanetBehaviour planet;
            public Vector3 pinchPoint;
            public bool isPinching;

            public void Clear()
            {
                planet = null;
                isPinching = false;
            }
        }
    }
}
