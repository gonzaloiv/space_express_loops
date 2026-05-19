using UnityEngine;
using DigitalLove.Global;
using System;
using DigitalLove.Game.Planets;
using DigitalLove.Game.UI;
using DigitalLove.VFX;

namespace DigitalLove.Game.Spaceships
{
    [RequireComponent(typeof(TravellerPathFollower))]
    public class TravellerBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private ResourcePanel lettersPanel;

        [SerializeField] private Renderer rend;
        [SerializeField] private AudioSource startMoveAudioSource;
        [SerializeField] private AudioSource loadedAudioSource;

        [SerializeField] private LayerMask layerMask;
        [SerializeField] private DetachedParticlePlayer detachedParticlePlayer;
        [SerializeField] private AudioSource hitAudioSource;

        private TravellerPathFollower pathFollower;
        private TravellerPathFollower PathFollower => pathFollower ??= GetComponent<TravellerPathFollower>();
        private bool isCollisionActive;
        private PlanetBehaviour arrivalPlanet;

        public bool IsFollowingPath => PathFollower.IsFollowingPath;

        public void Hide()
        {
            isCollisionActive = false;
            body.SetActive(false);  
        }

        public void HideAndCancelPath()
        {
            PathFollower.CancelFollowing();
            Hide();
        }

        public void ShowEmpty()
        {
            body.SetActive(true);
            lettersPanel.Hide();
            startMoveAudioSource.Play();
        }

        public void ShowLoaded(int letters)
        {
            body.SetActive(true);
            lettersPanel.ShowLetters(letters, 0);
            startMoveAudioSource.Play();
            loadedAudioSource.Play();
        }

        public void FollowPath(Vector3[] positions, PlanetBehaviour pickupPlanet, Action<bool> onPathEnded)
        {
            detachedParticlePlayer.ResetToParent();
            arrivalPlanet = pickupPlanet;
            isCollisionActive = true;
            PathFollower.FollowPath(positions, success =>
            {
                isCollisionActive = false;
                arrivalPlanet = null;
                onPathEnded?.Invoke(success);
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isCollisionActive)
                return;

            if (other.attachedRigidbody == null || !layerMask.Contains(other.attachedRigidbody.gameObject))
                return;

            PlanetBehaviour planet = other.attachedRigidbody.GetComponent<PlanetBehaviour>();
            if (planet == null)
                return;

            isCollisionActive = false;
            hitAudioSource.Play();
            detachedParticlePlayer.PlayAt();
            Hide();

            if (arrivalPlanet != null && planet == arrivalPlanet)
                PathFollower.EndWithSuccess();
            else
                PathFollower.EndWithFailure();
        }
    }
}
