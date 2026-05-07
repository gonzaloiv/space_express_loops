using UnityEngine;
using DigitalLove.Global;
using System;
using DigitalLove.Game.Planets;
using DigitalLove.Game.UI;

namespace DigitalLove.Game.Spaceships
{
    [RequireComponent(typeof(TravellerPathFollower))]
    public class TravellerBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private ResourcePanel lettersPanel;

        [SerializeField] private Renderer rend;
        [SerializeField] private ColorValue defaultColor;
        [SerializeField] private ColorValue loadedColor;
        [SerializeField] private AudioSource startMoveAudioSource;
        [SerializeField] private AudioSource loadedAudioSource;

        [SerializeField] private LayerMask layerMask;
        [SerializeField] private ParticleSystem ps;
        [SerializeField] private AudioSource hitAudioSource;

        private TravellerPathFollower pathFollower;
        private TravellerPathFollower PathFollower => pathFollower ??= GetComponent<TravellerPathFollower>();

        public void Hide()
        {
            PathFollower.StopFollowing();
            body.SetActive(false);
        }

        public void ShowEmpty()
        {
            body.SetActive(true);
            rend.material.color = defaultColor.value;
            lettersPanel.Hide();
            startMoveAudioSource.Play();
        }

        public void ShowLoaded(int letters)
        {
            body.SetActive(true);
            rend.material.color = loadedColor.value;
            lettersPanel.ShowLetters(letters, 0);
            startMoveAudioSource.Play();
            loadedAudioSource.Play();
        }

        public void FollowPath(Vector3[] positions, Action<bool> onPathEnded)
        {
            pathFollower.FollowPath(positions, onPathEnded);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (layerMask.Contains(other.gameObject))
            {
                PlanetBehaviour planet = other.gameObject.GetComponent<PlanetBehaviour>();
                if (planet != null)
                {
                    hitAudioSource.Play();
                    pathFollower.EndWithFailure();
                    Hide();
                    ps.Play();
                }
            }
        }
    }
}
