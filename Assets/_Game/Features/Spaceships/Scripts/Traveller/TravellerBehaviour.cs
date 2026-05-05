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
        [SerializeField] private Renderer rend;
        [SerializeField] private ColorValue defaultColor;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private ParticleSystem ps;
        [SerializeField] private LettersPanel lettersPanel;

        private Color loadedColor;

        private TravellerPathFollower pathFollower;
        private TravellerPathFollower PathFollower => pathFollower ??= GetComponent<TravellerPathFollower>();

        public void SetColor(Color color)
        {
            loadedColor = color;
        }

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
        }

        public void ShowLoaded(int letters)
        {
            body.SetActive(true);
            rend.material.color = loadedColor;
            lettersPanel.ShowLetters(letters, 0);
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
                    Debug.LogWarning($"Collision with planet: {planet.Id}");
                pathFollower.EndWithFailure();
                Hide();
                ps.Play();
            }
        }
    }
}
